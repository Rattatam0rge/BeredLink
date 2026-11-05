/* Viser riktige felt i registreringsskjemaet avhengig av valgt rolle. */
(function () {
  'use strict';

  var pub = document.getElementById('public-fields');
  var prov = document.getElementById('provider-fields');
  var rolePublic = document.getElementById('role-public');
  var roleProvider = document.getElementById('role-provider');
  if (!pub || !prov || !rolePublic || !roleProvider) return;

  function update() {
    var isPublic = rolePublic.checked;
    pub.hidden = !isPublic;
    prov.hidden = isPublic;
  }

  rolePublic.addEventListener('change', update);
  roleProvider.addEventListener('change', update);
  update();
})();
