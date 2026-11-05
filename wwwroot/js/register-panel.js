/* Paneler over kartet: registrer behov/ressurs, endre ressurs og sett organisasjonens posisjon.
 * Stedet velges i det samme hovedkartet. Kun ett panel er åpent om gangen.
 * På mobil er panelet et «bunnark» som kan minimeres, slik at hele kartet er synlig mens man velger sted. */
(function () {
  'use strict';

  var panels = Array.prototype.slice.call(document.querySelectorAll('.reg-panel'));
  if (panels.length === 0 || !window.FM || !FM.map) return;

  var map = FM.map;
  var wrap = document.querySelector('.map-wrap');
  var mapEl = document.getElementById('map');
  var registry = {};
  var lastOpener = null;

  function isMobile() { return window.matchMedia('(max-width: 575.98px)').matches; }
  function pad(n) { return String(n).padStart(2, '0'); }
  function localInput(d) {
    return d.getFullYear() + '-' + pad(d.getMonth() + 1) + '-' + pad(d.getDate()) + 'T' + pad(d.getHours()) + ':' + pad(d.getMinutes());
  }

  // Skalerer ned bildet i nettleseren (maks 1600 px) og lagrer det på nytt som JPEG.
  // Det gir raskere opplasting og fjerner EXIF-data som GPS-posisjon fra mobilbilder.
  // Serveren validerer uansett filtype og størrelse selv.
  function processImage(file) {
    return new Promise(function (resolve) {
      if (!/^image\/(jpeg|png|webp)$/.test(file.type)) { resolve(file); return; }
      var url = URL.createObjectURL(file);
      var img = new Image();
      img.onload = function () {
        var scale = Math.min(1, 1600 / Math.max(img.width, img.height));
        var canvas = document.createElement('canvas');
        canvas.width = Math.round(img.width * scale);
        canvas.height = Math.round(img.height * scale);
        var ctx = canvas.getContext('2d');
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
        canvas.toBlob(function (blob) { URL.revokeObjectURL(url); resolve(blob || file); }, 'image/jpeg', 0.85);
      };
      img.onerror = function () { URL.revokeObjectURL(url); resolve(file); };
      img.src = url;
    });
  }

  function initPanel(panel) {
    var form = panel.querySelector('form');
    var closeBtn = panel.querySelector('.reg-close');
    var minBtn = panel.querySelector('.reg-min');
    var titleEl = panel.querySelector('.reg-title');
    var submitBtn = panel.querySelector('.reg-submit');
    var latIn = panel.querySelector('[name="Latitude"]');
    var lngIn = panel.querySelector('[name="Longitude"]');
    var locText = panel.querySelector('.reg-loc-text');
    var generalError = panel.querySelector('.reg-general-error');
    var imageInput = form.querySelector('[name="Image"]');
    var removeInput = form.querySelector('[name="RemoveImage"]');
    var imageWrap = panel.querySelector('.reg-image-wrap');
    var imagePreview = panel.querySelector('.reg-image-preview');
    var imageClear = panel.querySelector('.reg-image-clear');
    var defaultSubmitLabel = submitBtn.textContent;
    var previewUrl = null;
    var existingImage = false;
    var editId = null;
    var pin = null;
    var isOpen = false;

    function setDefaults() {
      var deadline = form.querySelector('[name="Deadline"]');
      var from = form.querySelector('[name="AvailableFrom"]');
      if (deadline && !deadline.value) deadline.value = localInput(new Date(Date.now() + 24 * 3600 * 1000));
      if (from && !from.value) from.value = localInput(new Date());
    }

    // ---- Registrere eller endre ----
    function setMode(id) {
      editId = id;
      if (titleEl && panel.dataset.titleEdit) titleEl.textContent = id ? panel.dataset.titleEdit : panel.dataset.titleNew;
      submitBtn.textContent = id && panel.dataset.submitEdit ? panel.dataset.submitEdit : (panel.dataset.submitNew || defaultSubmitLabel);
    }

    // ---- Minimere panelet (mobil) ----
    function setMinimized(min) {
      panel.classList.toggle('minimized', min);
      if (minBtn) {
        minBtn.textContent = min ? 'Vis skjema' : 'Vis kart';
        minBtn.setAttribute('aria-expanded', String(!min));
      }
    }
    if (minBtn) minBtn.addEventListener('click', function () { setMinimized(!panel.classList.contains('minimized')); });

    // ---- Bilde ----
    function clearImage() {
      if (!imageInput) return;
      imageInput.value = '';
      if (previewUrl) { URL.revokeObjectURL(previewUrl); previewUrl = null; }
      imageWrap.hidden = true;
      imagePreview.removeAttribute('src');
    }

    if (imageInput) {
      imageInput.addEventListener('change', function () {
        showError('Image', '');
        var file = imageInput.files[0];
        if (!file) { clearImage(); return; }
        if (file.size > 20 * 1024 * 1024) { clearImage(); showError('Image', 'Bildet er for stort.'); return; }
        if (previewUrl) URL.revokeObjectURL(previewUrl);
        previewUrl = URL.createObjectURL(file);
        imagePreview.src = previewUrl;
        imageWrap.hidden = false;
        if (removeInput) removeInput.value = 'false';
      });
      imageClear.addEventListener('click', function () {
        if (existingImage && removeInput) { removeInput.value = 'true'; existingImage = false; }
        clearImage();
      });
    }

    // ---- Valg av sted i hovedkartet ----
    function showError(field, msg) {
      var el = form.querySelector('[data-error-for="' + field + '"]');
      if (el) el.textContent = msg;
    }

    // Flytter kartet slik at punktet er synlig. På mobil dekker panelet nederste del, så punktet legges i øvre del.
    function showPoint(lat, lng) {
      var z = Math.max(map.getZoom(), 13);
      if (isMobile() && !panel.classList.contains('minimized')) {
        var shifted = map.project([lat, lng], z).add([0, map.getSize().y * 0.30]);
        map.setView(map.unproject(shifted, z), z);
      } else {
        map.setView([lat, lng], z);
      }
    }

    function setPoint(lat, lng, pan) {
      latIn.value = lat.toFixed(6);
      lngIn.value = lng.toFixed(6);
      locText.textContent = 'Valgt posisjon: ' + lat.toFixed(5) + ', ' + lng.toFixed(5);
      locText.classList.add('has-point');
      showError('Latitude', '');

      if (!pin) {
        pin = L.marker([lat, lng], { icon: FM.icon('pick'), draggable: true, keyboard: true }).addTo(map);
        pin.on('dragend', function () { var p = pin.getLatLng(); setPoint(p.lat, p.lng, false); });
      } else {
        pin.setLatLng([lat, lng]);
      }

      // Valgte man sted mens panelet var minimert, vises skjemaet igjen
      if (panel.classList.contains('minimized')) { setMinimized(false); pan = true; }
      if (pan) showPoint(lat, lng);
    }

    function onMapClick(e) { setPoint(e.latlng.lat, e.latlng.lng, false); }

    function clearPoint() {
      latIn.value = '';
      lngIn.value = '';
      locText.textContent = 'Trykk på kartet for å velge sted';
      locText.classList.remove('has-point');
      if (pin) { map.removeLayer(pin); pin = null; }
    }

    // ---- Feilvisning ----
    function clearErrors() {
      form.querySelectorAll('.field-error').forEach(function (el) { el.textContent = ''; });
      generalError.hidden = true;
      generalError.textContent = '';
    }

    function showErrors(errors) {
      var general = [];
      Object.keys(errors).forEach(function (key) {
        var msg = errors[key].join(' ');
        var field = key === 'Longitude' ? 'Latitude' : key;
        var el = key ? form.querySelector('[data-error-for="' + field + '"]') : null;
        if (el) el.textContent = msg; else general.push(msg);
      });
      if (general.length) {
        generalError.textContent = general.join(' ');
        generalError.hidden = false;
      }
    }

    // ---- Åpne/lukke ----
    function open() {
      Object.keys(registry).forEach(function (id) {
        if (registry[id] !== api && registry[id].isOpen()) registry[id].close(false);
      });
      isOpen = true;
      panel.hidden = false;
      setMinimized(false);
      wrap.classList.add('reg-open');
      mapEl.classList.add('picking');
      map.on('click', onMapClick);
      setDefaults();
      var first = form.querySelector('select, input:not([type="hidden"]):not([type="radio"])');
      if (first && !isMobile()) first.focus();   // på mobil unngår vi at tastaturet dekker kartet
    }

    function close(restoreFocus) {
      isOpen = false;
      panel.hidden = true;
      wrap.classList.remove('reg-open');
      mapEl.classList.remove('picking');
      map.off('click', onMapClick);
      clearErrors();
      clearPoint();
      clearImage();
      existingImage = false;
      form.reset();
      if (removeInput) removeInput.value = 'false';
      setMode(null);
      if (restoreFocus !== false && lastOpener) lastOpener.focus();
    }

    // Åpner panelet i endre-modus og fyller det med ressursens data
    function openEdit(id) {
      if (!form.dataset.getUrl) return;
      fetch(form.dataset.getUrl + '/' + id, { credentials: 'same-origin', headers: { Accept: 'application/json' } })
        .then(function (res) {
          var ct = res.headers.get('content-type') || '';
          if (!res.ok || ct.indexOf('json') < 0) throw new Error('Fant ikke ressursen.');
          return res.json();
        })
        .then(function (d) {
          open();
          setMode(id);
          form.elements['Type'].value = d.type;
          form.elements['Description'].value = d.description || '';
          form.elements['AvailableFrom'].value = d.availableFrom || '';
          form.elements['AvailableTo'].value = d.availableTo || '';
          if (d.imageUrl) {
            existingImage = true;
            imagePreview.src = d.imageUrl;
            imageWrap.hidden = false;
          }
          setPoint(d.latitude, d.longitude, true);
        })
        .catch(function (err) { FM.toast(err.message, 'danger'); });
    }

    closeBtn.addEventListener('click', function () { close(true); });
    panel.addEventListener('keydown', function (e) { if (e.key === 'Escape') close(true); });

    panel.querySelector('.reg-mylocation').addEventListener('click', function () {
      if (!navigator.geolocation) {
        locText.textContent = 'Enheten din støtter ikke posisjon. Trykk i kartet i stedet.';
        return;
      }
      locText.textContent = 'Henter posisjon…';
      navigator.geolocation.getCurrentPosition(
        function (pos) { setPoint(pos.coords.latitude, pos.coords.longitude, true); },
        function () { locText.textContent = 'Fikk ikke tilgang til posisjonen. Trykk i kartet i stedet.'; },
        { enableHighAccuracy: true, timeout: 10000 }
      );
    });

    // ---- Send inn ----
    function setBusy(busy) {
      submitBtn.disabled = busy;
      if (busy) submitBtn.textContent = 'Lagrer…';
      else setMode(editId);
    }

    // Bygger skjemadata, med nedskalert bilde hvis et er valgt
    function buildFormData() {
      var fd = new FormData(form);   // inkluderer antiforgery-token fra skjemaet
      if (!imageInput) return Promise.resolve(fd);

      var file = imageInput.files[0];
      if (!file) { fd.delete('Image'); return Promise.resolve(fd); }
      return processImage(file).then(function (blob) {
        fd.set('Image', blob, 'bilde.jpg');
        return fd;
      });
    }

    form.addEventListener('submit', function (e) {
      e.preventDefault();
      clearErrors();

      if (!latIn.value || !lngIn.value) {
        if (panel.classList.contains('minimized')) setMinimized(false);
        showError('Latitude', 'Velg et sted på kartet.');
        locText.scrollIntoView({ block: 'nearest' });
        return;
      }
      if (!form.reportValidity()) return;

      var url = editId ? form.dataset.updateUrl + '/' + editId : form.action;

      setBusy(true);
      buildFormData()
        .then(function (fd) {
          return fetch(url, {
            method: 'POST',
            body: fd,
            credentials: 'same-origin',
            headers: { 'X-Requested-With': 'XMLHttpRequest', Accept: 'application/json' }
          });
        })
        .then(function (res) {
          var ct = res.headers.get('content-type') || '';
          if (ct.indexOf('json') < 0) throw new Error('Økten din kan ha utløpt, eller bildet er for stort. Last siden på nytt og prøv igjen.');
          return res.json().then(function (data) { return { ok: res.ok, data: data }; });
        })
        .then(function (r) {
          if (r.ok && r.data.success) {
            var lat = parseFloat(latIn.value), lng = parseFloat(lngIn.value);
            close(false);
            FM.toast(r.data.message, 'success', r.data.url, r.data.linkText);
            FM.reload();
            map.setView([lat, lng], Math.max(map.getZoom(), 13));
          } else {
            showErrors(r.data.errors || {});
          }
        })
        .catch(function (err) { FM.toast(err.message, 'danger'); })
        .finally(function () { setBusy(false); });
    });

    var api = { open: open, close: close, openEdit: openEdit, isOpen: function () { return isOpen; } };
    registry[panel.id] = api;
  }

  panels.forEach(initPanel);

  // Knapper med data-open-panel åpner tilhørende panel
  document.querySelectorAll('[data-open-panel]').forEach(function (btn) {
    btn.addEventListener('click', function () {
      var target = registry[btn.getAttribute('data-open-panel')];
      if (!target) return;
      lastOpener = btn;
      target.open();
    });
  });

  // Endre egen ressurs: brukes fra kartpopup (FM.openEdit) og fra listen («/?edit=ID»)
  FM.openEdit = function (id) {
    var p = registry['register-panel'];
    if (p) p.openEdit(id);
  };

  var query = new URLSearchParams(window.location.search);
  if (query.get('register') === '1' && registry['register-panel']) {
    registry['register-panel'].open();
  }
  var editParam = parseInt(query.get('edit'), 10);
  if (!isNaN(editParam)) FM.openEdit(editParam);
})();
