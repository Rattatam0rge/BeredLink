/* Små UI-hjelpere uten inline-skript (kreves av Content-Security-Policy). */
(function () {
  'use strict';

  // <form data-confirm="Er du sikker?"> ber om bekreftelse før innsending
  document.addEventListener('submit', function (e) {
    var form = e.target;
    var message = form.getAttribute && form.getAttribute('data-confirm');
    if (message && !window.confirm(message)) e.preventDefault();
  });

  // <button data-print> skriver ut siden
  document.addEventListener('click', function (e) {
    var el = e.target.closest ? e.target.closest('[data-print]') : null;
    if (el) window.print();
  });
})();
