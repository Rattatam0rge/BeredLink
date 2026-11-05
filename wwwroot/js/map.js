(function () {
  'use strict';

  var mapEl = document.getElementById('map');
  if (!mapEl) return;

  var map = L.map('map').setView(FM.NORWAY, 5);
  FM.addBaseLayer(map);
  FM.map = map;

  // Behov og aktører vises alltid enkeltvis. Ressurser samles i klynger når mange ligger tett.
  var plainLayer = L.layerGroup().addTo(map);
  var cluster = L.markerClusterGroup({
    maxClusterRadius: 45,
    disableClusteringAtZoom: 15,
    showCoverageOnHover: false,
    spiderfyOnMaxZoom: true,
    iconCreateFunction: function (c) { return FM.clusterIcon(c.getChildCount()); }
  }).addTo(map);

  var all = [];
  var firstLoad = true;
  var state = { acute: true, planned: true, resource: true, actor: true, type: '' };

  var statusEl = document.getElementById('map-status');
  var typeSelect = document.getElementById('f-type');

  // ---- Tegnforklaring: åpen som standard, kan minimeres med streken ----
  var legendBox = document.getElementById('legend-box');
  var legend = document.getElementById('legend');
  var legendToggle = document.getElementById('legend-toggle');

  var KIND_LABELS = {
    acute: 'Akutt behov', planned: 'Planlagt behov',
    resource: 'Tilgjengelig ressurs', actor: 'Offentlig aktør'
  };
  var kindsBox = document.getElementById('legend-kinds');
  (legendBox.dataset.kinds || 'resource').split(',').forEach(function (k) {
    var row = document.createElement('div');
    row.className = 'legend-item';
    var ic = document.createElement('span');
    ic.innerHTML = FM.markerSvg(k, null, 30);
    var label = document.createElement('span');
    label.textContent = KIND_LABELS[k];
    row.append(ic, label);
    kindsBox.append(row);
  });

  // Typer som finnes på kartet akkurat nå, med samme symbol som i markøren
  var typesWrap = document.getElementById('legend-types-wrap');
  var typesBox = document.getElementById('legend-types');
  function renderTypeLegend() {
    var types = Array.from(new Set(all
      .filter(function (m) { return m.kind !== 'actor'; })
      .map(function (m) { return m.type; }))).sort();
    typesBox.textContent = '';
    types.forEach(function (t) {
      var row = document.createElement('div');
      row.className = 'legend-item';
      var ic = document.createElement('span');
      ic.innerHTML = FM.typeIconSvg(t);
      var label = document.createElement('span');
      label.textContent = t;
      row.append(ic, label);
      typesBox.append(row);
    });
    typesWrap.hidden = types.length === 0;
  }

  function setLegend(open, remember) {
    legend.hidden = !open;
    legendBox.classList.toggle('minimized', !open);
    legendToggle.setAttribute('aria-expanded', String(open));
    legendToggle.setAttribute('aria-label', open ? 'Minimer tegnforklaring' : 'Åpne tegnforklaring');
    if (remember) {
      try { localStorage.setItem('fm.legend', open ? 'open' : 'min'); } catch (e) { /* ignorer */ }
    }
  }
  var savedLegend = null;
  try { savedLegend = localStorage.getItem('fm.legend'); } catch (e) { /* ignorer */ }
  // Åpen som standard på store skjermer. På mobil starter den minimert for ikke å dekke kartet.
  var smallScreen = window.matchMedia('(max-width: 575.98px)').matches;
  setLegend(savedLegend === null ? !smallScreen : savedLegend !== 'min', false);
  legendToggle.addEventListener('click', function () { setLegend(legend.hidden, true); });

  // ---- Popup (bygges med DOM/textContent: brukerdata tolkes aldri som HTML) ----
  function row(label, value) {
    var d = document.createElement('div');
    var l = document.createElement('span');
    l.className = 'text-body-secondary';
    l.textContent = label + ': ';
    var v = document.createElement('span');
    v.textContent = value;
    d.append(l, v);
    return d;
  }

  function linkRow(label, text, href) {
    var d = document.createElement('div');
    var l = document.createElement('span');
    l.className = 'text-body-secondary';
    l.textContent = label + ': ';
    var a = document.createElement('a');
    a.textContent = text;
    a.href = href;
    d.append(l, a);
    return d;
  }

  function buildPopup(m) {
    var root = document.createElement('div');

    if (m.imageUrl) {
      var link = document.createElement('a');
      link.href = m.imageUrl;
      link.target = '_blank';
      link.rel = 'noopener';
      var img = document.createElement('img');
      img.className = 'popup-img';
      img.src = m.imageUrl;
      img.alt = 'Bilde av ' + m.type;
      img.loading = 'lazy';
      link.append(img);
      root.append(link);
    }

    var h = document.createElement('strong');
    h.textContent = m.title;
    root.append(h);
    if (m.status) root.append(row('Status', m.status));
    if (m.when) root.append(row('Tid', m.when));
    if (m.contact) root.append(row('Kontakt', m.contact));
    if (m.phone) root.append(linkRow('Telefon', m.phone, 'tel:' + m.phone.replace(/[^+0-9]/g, '')));
    if (m.email) root.append(linkRow('E-post', m.email, 'mailto:' + m.email));

    if (m.canEdit && FM.openEdit) {
      var edit = document.createElement('button');
      edit.type = 'button';
      edit.className = 'btn btn-outline-dark btn-sm mt-2 me-2';
      edit.textContent = 'Endre';
      edit.addEventListener('click', function () { map.closePopup(); FM.openEdit(m.id); });
      root.append(edit);
    }

    if (m.kind === 'need' && focusBox) {
      var near = document.createElement('button');
      near.type = 'button';
      near.className = 'btn btn-outline-dark btn-sm mt-2 me-2';
      near.textContent = 'Ressurser i nærheten';
      near.addEventListener('click', function () { map.closePopup(); startFocus(m); });
      root.append(near);
    }
    if (m.url) {
      var a = document.createElement('a');
      a.className = 'btn btn-primary btn-sm mt-2 text-white';
      a.href = m.url;
      a.textContent = 'Åpne detaljer';
      root.append(a);
    }
    return root;
  }

  // ---- Radius rundt et valgt behov ----
  var focusBox = document.getElementById('focus-box');
  var focus = null;
  var focusCircle = null;
  var RADII = [[500, '500 m'], [1000, '1 km'], [2000, '2 km'], [5000, '5 km'], [10000, '10 km']];

  if (focusBox) {
    var radiiBox = document.getElementById('focus-radii');
    var suitableBox = document.getElementById('focus-suitable');
    var focusTitle = document.getElementById('focus-title');
    var focusOpen = document.getElementById('focus-open');

    RADII.forEach(function (r) {
      var b = document.createElement('button');
      b.type = 'button';
      b.className = 'btn btn-outline-dark';
      b.textContent = r[1];
      b.setAttribute('data-radius', String(r[0]));
      b.addEventListener('click', function () { if (focus) { focus.radius = r[0]; applyFocus(true); } });
      radiiBox.append(b);
    });

    suitableBox.addEventListener('change', function () { if (focus) { focus.onlySuitable = suitableBox.checked; applyFocus(false); } });
    document.getElementById('focus-close').addEventListener('click', stopFocus);
  }

  function startFocus(m) {
    focus = { id: m.id, lat: m.lat, lng: m.lng, url: m.url, suitable: m.suitable || [], radius: 1000, onlySuitable: true };
    focusTitle.textContent = m.title;
    suitableBox.checked = true;
    focusBox.hidden = false;
    document.querySelector('.map-wrap').classList.add('focus-open');
    applyFocus(true);
  }

  function stopFocus() {
    focus = null;
    if (focusCircle) { map.removeLayer(focusCircle); focusCircle = null; }
    if (focusBox) focusBox.hidden = true;
    document.querySelector('.map-wrap').classList.remove('focus-open');
    render();
  }

  function applyFocus(fit) {
    if (!focus) return;
    if (focusCircle) map.removeLayer(focusCircle);
    focusCircle = L.circle([focus.lat, focus.lng], {
      radius: focus.radius, color: '#000', weight: 3, dashArray: '8 6',
      fillColor: '#c62828', fillOpacity: 0.07, interactive: false
    }).addTo(map);

    radiiBox.querySelectorAll('button').forEach(function (b) {
      var active = parseInt(b.getAttribute('data-radius'), 10) === focus.radius;
      b.classList.toggle('btn-dark', active);
      b.classList.toggle('btn-outline-dark', !active);
      b.setAttribute('aria-pressed', String(active));
    });

    focusOpen.href = focus.url + '?radius=' + focus.radius + '&suitable=' + focus.onlySuitable;
    if (fit) map.fitBounds(focusCircle.getBounds(), { padding: [40, 40] });
    render();
  }

  // ---- Filter ----
  function inFocus(m) {
    if (!focus || m.kind !== 'resource') return true;
    if (map.distance([focus.lat, focus.lng], [m.lat, m.lng]) > focus.radius) return false;
    if (focus.onlySuitable && focus.suitable.length && focus.suitable.indexOf(m.type) < 0) return false;
    return true;
  }

  function visible(m) {
    return state[m.icon] && (!state.type || m.type === state.type) && inFocus(m);
  }

  function render() {
    plainLayer.clearLayers();
    cluster.clearLayers();
    var shown = all.filter(visible);

    shown.forEach(function (m) {
      var marker = L.marker([m.lat, m.lng], { icon: FM.icon(m.icon, m.type), title: m.title, alt: m.title, keyboard: true });
      marker.bindPopup(buildPopup(m), { maxWidth: 270 });
      if (m.kind === 'resource') cluster.addLayer(marker); else plainLayer.addLayer(marker);
    });

    var needs = shown.filter(function (m) { return m.kind === 'need'; }).length;
    var resources = shown.filter(function (m) { return m.kind === 'resource'; }).length;
    statusEl.textContent = needs + ' behov og ' + resources + ' ressurser vises';

    if (focus) {
      document.getElementById('focus-count').textContent =
        resources + (resources === 1 ? ' ressurs' : ' ressurser') + ' innen ' + (focus.radius >= 1000 ? (focus.radius / 1000) + ' km' : focus.radius + ' m');
    }
  }

  function refreshTypeOptions() {
    var types = Array.from(new Set(all.map(function (m) { return m.type; }))).sort();
    var current = state.type;
    typeSelect.length = 1;
    types.forEach(function (t) {
      var o = document.createElement('option');
      o.value = t;
      o.textContent = t;
      typeSelect.append(o);
    });
    typeSelect.value = types.indexOf(current) >= 0 ? current : '';
    state.type = typeSelect.value;
  }

  var toggle = document.getElementById('filter-toggle');
  var panel = document.getElementById('filter-panel');
  toggle.addEventListener('click', function () {
    var open = panel.hidden;
    panel.hidden = !open;
    toggle.setAttribute('aria-expanded', String(open));
  });

  ['acute', 'planned', 'resource', 'actor'].forEach(function (k) {
    var el = document.getElementById('f-' + k);
    if (el) el.addEventListener('change', function (e) { state[k] = e.target.checked; render(); });
  });
  typeSelect.addEventListener('change', function () { state.type = typeSelect.value; render(); });

  document.getElementById('f-reset').addEventListener('click', function () {
    state.acute = state.planned = state.resource = state.actor = true;
    state.type = '';
    ['acute', 'planned', 'resource', 'actor'].forEach(function (k) {
      var el = document.getElementById('f-' + k);
      if (el) el.checked = true;
    });
    typeSelect.value = '';
    if (focus) stopFocus(); else render();
  });

  // Utskrift av ressursliste for dagens kartutsnitt
  var printBtn = document.getElementById('print-list');
  if (printBtn) {
    printBtn.addEventListener('click', function () {
      var b = map.getBounds();
      var q = new URLSearchParams({
        minLat: b.getSouth().toFixed(5), maxLat: b.getNorth().toFixed(5),
        minLng: b.getWest().toFixed(5), maxLng: b.getEast().toFixed(5)
      });
      if (state.type) q.set('type', state.type);
      window.open('/Resource/Print?' + q.toString(), '_blank', 'noopener');
    });
  }

  // ---- Data ----
  function load() {
    return fetch('/api/map/markers', { credentials: 'same-origin', headers: { Accept: 'application/json' } })
      .then(function (res) {
        var ct = res.headers.get('content-type') || '';
        if (!res.ok || ct.indexOf('json') < 0) throw new Error('Kunne ikke hente kartdata.');
        return res.json();
      })
      .then(function (data) {
        all = data;
        refreshTypeOptions();
        renderTypeLegend();

        // Valgt behov kan ha blitt løst mens kartet var åpent
        if (focus && !all.some(function (m) { return m.kind === 'need' && m.id === focus.id; })) stopFocus();
        else render();

        if (firstLoad) {
          firstLoad = false;
          var q = new URLSearchParams(window.location.search);
          var lat = parseFloat(q.get('lat')), lng = parseFloat(q.get('lng')), z = parseInt(q.get('z'), 10);
          if (!isNaN(lat) && !isNaN(lng)) {
            map.setView([lat, lng], isNaN(z) ? 14 : z);
          } else if (all.length > 0) {
            map.fitBounds(all.map(function (m) { return [m.lat, m.lng]; }), { padding: [60, 60], maxZoom: 12 });
          }
        }
      })
      .catch(function (err) { statusEl.textContent = err.message; });
  }

  FM.reload = load;
  load();
  // Hold kartet oppdatert uten manuell reload (felles situasjonsforståelse)
  setInterval(function () { if (!document.hidden) load(); }, 30000);
})();
