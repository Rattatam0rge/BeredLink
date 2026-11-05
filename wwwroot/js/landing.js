/* Flytende markører på startsiden (samme symboler som på kartet). */
(function () {
  'use strict';

  var box = document.getElementById('landing-floats');
  if (!box || !window.FM) return;

  // [form, type, x%, y%, forsinkelse i sekunder]
  var items = [
    ['resource', 'Traktor', 12, 22, 0.2],
    ['acute', 'Strøm/aggregat', 84, 18, 0.5],
    ['planned', 'Snørydding', 22, 70, 0.8],
    ['resource', 'Lastebil', 78, 66, 1.1],
    ['actor', 'Offentlig aktør', 50, 10, 0.4],
    ['resource', 'Drone og droneoperatør', 91, 44, 1.4],
    ['acute', 'Evakuering', 7, 46, 0.7],
    ['resource', 'Båt', 66, 86, 1.0]
  ];

  items.forEach(function (it, i) {
    var el = document.createElement('span');
    el.className = 'landing-float';
    el.style.left = it[2] + '%';
    el.style.top = it[3] + '%';
    el.style.animationDelay = (0.4 + it[4]) + 's, ' + (i * 0.7) + 's';
    el.innerHTML = FM.markerSvg(it[0], it[1], 48);
    box.append(el);
  });
})();
