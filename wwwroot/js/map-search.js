/* Søk etter sted, adresse eller postnummer via Kartverkets åpne API-er (Geonorge). */
(function () {
  'use strict';

  var form = document.getElementById('search-form');
  if (!form || !window.FM || !FM.map) return;

  var map = FM.map;
  var input = document.getElementById('search-input');
  var list = document.getElementById('search-results');

  var results = [];
  var active = -1;
  var timer = null;
  var controller = null;
  var requestId = 0;
  var marker = null;

  function getJson(url, signal) {
    return fetch(url, { signal: signal, headers: { Accept: 'application/json' } }).then(function (r) {
      if (!r.ok) throw new Error('Søket feilet');
      return r.json();
    });
  }

  function pickLat(p) { return p.nord !== undefined ? p.nord : p.lat; }
  function pickLng(p) { return p['øst'] !== undefined ? p['øst'] : (p.ost !== undefined ? p.ost : p.lon); }

  function parsePlaces(data) {
    return (data.navn || []).map(function (n) {
      var p = n.representasjonspunkt;
      if (!p) return null;
      var name = n['skrivemåte'] || (n.stedsnavn && n.stedsnavn[0] && n.stedsnavn[0]['skrivemåte']);
      var kommune = (n.kommuner || []).map(function (k) { return k.kommunenavn; }).join(', ');
      return {
        label: name || '',
        sub: [n.navneobjekttype, kommune].filter(Boolean).join(' · '),
        lat: pickLat(p), lng: pickLng(p), zoom: 12
      };
    }).filter(function (x) { return x && x.label && x.lat !== undefined && x.lng !== undefined; });
  }

  function parseAddresses(data) {
    return (data.adresser || []).map(function (a) {
      var p = a.representasjonspunkt;
      if (!p) return null;
      return {
        label: a.adressetekst,
        sub: [[a.postnummer, a.poststed].filter(Boolean).join(' '), a.kommunenavn].filter(Boolean).join(' · '),
        lat: p.lat, lng: p.lon, zoom: 16
      };
    }).filter(Boolean);
  }

  // Postnummer: snitt av adressene i området
  function parsePostcode(data) {
    var a = (data.adresser || []).filter(function (x) { return x.representasjonspunkt; });
    if (a.length === 0) return [];
    var lat = 0, lng = 0;
    a.forEach(function (x) { lat += x.representasjonspunkt.lat; lng += x.representasjonspunkt.lon; });
    return [{
      label: a[0].postnummer + ' ' + (a[0].poststed || ''),
      sub: 'Postnummer · ' + (a[0].kommunenavn || ''),
      lat: lat / a.length, lng: lng / a.length, zoom: 13
    }];
  }

  function searchPlaces(q, signal) {
    var base = 'https://ws.geonorge.no/stedsnavn/v1/navn?treffPerSide=6&sok=';
    return getJson(base + encodeURIComponent(q + '*'), signal).then(parsePlaces).catch(function () { return []; })
      .then(function (r) {
        if (r.length) return r;
        return getJson(base + encodeURIComponent(q) + '&fuzzy=true', signal).then(parsePlaces).catch(function () { return []; });
      });
  }

  function searchAddresses(q, signal) {
    return getJson('https://ws.geonorge.no/adresser/v1/sok?treffPerSide=6&fuzzy=true&sok=' + encodeURIComponent(q), signal)
      .then(parseAddresses).catch(function () { return []; });
  }

  function searchPostcode(q, signal) {
    return getJson('https://ws.geonorge.no/adresser/v1/sok?treffPerSide=100&postnummer=' + encodeURIComponent(q), signal)
      .then(parsePostcode).catch(function () { return []; });
  }

  function run(q) {
    if (controller) controller.abort();
    controller = new AbortController();
    var id = ++requestId;
    var s = controller.signal;

    var jobs = /^\d{4}$/.test(q)
      ? [searchPostcode(q, s)]
      : [searchPlaces(q, s), searchAddresses(q, s)];

    return Promise.all(jobs).then(function (arr) {
      if (id !== requestId) return null;       // utdatert svar
      return [].concat.apply([], arr);
    });
  }

  // ---- Visning ----
  function closeList() {
    list.hidden = true;
    input.setAttribute('aria-expanded', 'false');
    active = -1;
  }

  function setActive(i) {
    var items = list.querySelectorAll('[data-index]');
    items.forEach(function (el) { el.classList.remove('active-result'); el.setAttribute('aria-selected', 'false'); });
    active = i;
    if (i >= 0 && items[i]) {
      items[i].classList.add('active-result');
      items[i].setAttribute('aria-selected', 'true');
      items[i].scrollIntoView({ block: 'nearest' });
    }
  }

  function render(items) {
    results = items;
    list.textContent = '';
    if (items.length === 0) {
      var none = document.createElement('li');
      none.className = 'list-group-item text-body-secondary';
      none.textContent = 'Ingen treff';
      list.append(none);
    }
    items.forEach(function (it, i) {
      var li = document.createElement('li');
      li.className = 'list-group-item';
      li.setAttribute('role', 'option');
      li.setAttribute('data-index', String(i));
      var main = document.createElement('div');
      main.textContent = it.label;
      var sub = document.createElement('div');
      sub.className = 'sub';
      sub.textContent = it.sub;
      li.append(main, sub);
      // mousedown (ikke click) slik at feltet ikke mister fokus før valget registreres
      li.addEventListener('mousedown', function (e) { e.preventDefault(); choose(i); });
      list.append(li);
    });
    list.hidden = false;
    input.setAttribute('aria-expanded', 'true');
    active = -1;
  }

  function choose(i) {
    var it = results[i];
    if (!it) return;
    input.value = it.label;
    closeList();

    if (marker) map.removeLayer(marker);
    marker = L.circleMarker([it.lat, it.lng], {
      radius: 13, color: '#1565c0', weight: 4, fillColor: '#ffffff', fillOpacity: 0.5
    }).addTo(map);
    marker.bindTooltip(it.label);

    map.flyTo([it.lat, it.lng], it.zoom, { duration: 1.2 });
  }

  function doSearch(q, goFirst) {
    run(q).then(function (items) {
      if (items === null) return;
      if (goFirst && items.length) { results = items; choose(0); }
      else render(items);
    });
  }

  input.addEventListener('input', function () {
    clearTimeout(timer);
    var q = input.value.trim();
    if (q.length === 0 && marker) { map.removeLayer(marker); marker = null; }
    if (q.length < 2) { closeList(); return; }
    timer = setTimeout(function () { doSearch(q, false); }, 300);
  });

  input.addEventListener('keydown', function (e) {
    if (e.key === 'ArrowDown' && !list.hidden && results.length) {
      e.preventDefault();
      setActive(Math.min(active + 1, results.length - 1));
    } else if (e.key === 'ArrowUp' && !list.hidden && results.length) {
      e.preventDefault();
      setActive(Math.max(active - 1, 0));
    } else if (e.key === 'Escape') {
      closeList();
    }
  });

  form.addEventListener('submit', function (e) {
    e.preventDefault();
    var q = input.value.trim();
    if (q.length < 2) return;
    if (!list.hidden && results.length) { choose(active >= 0 ? active : 0); return; }
    doSearch(q, true);
  });

  document.addEventListener('click', function (e) { if (!form.contains(e.target)) closeList(); });
})();
