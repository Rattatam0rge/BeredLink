/* Felles kartoppsett: Kartverket-bakgrunn, markørsymboler, klyngeikon og toast.
 *
 * Symboler: form + farge viser hva det er (rød trekant = akutt behov, gul rombe = planlagt behov,
 * grønn sirkel = ressurs, blå firkant = offentlig aktør). Symbolet i midten viser typen.
 * Hver markør har hvit kant innerst og svart kant ytterst for kontrast mot alle bakgrunner.
 * Ikonene er fra Material Design Icons (Pictogrammers), Apache License 2.0.
 */
window.FM = (function () {
  'use strict';

  var NORWAY = [64.5, 11];

  var GLYPHS = {
    drone: "M5.5,1C8,1 10,3 10,5.5C10,6.38 9.75,7.2 9.31,7.9L9.41,8H14.59L14.69,7.9C14.25,7.2 14,6.38 14,5.5C14,3 16,1 18.5,1C21,1 23,3 23,5.5C23,8 21,10 18.5,10C17.62,10 16.8,9.75 16.1,9.31L15,10.41V13.59L16.1,14.69C16.8,14.25 17.62,14 18.5,14C21,14 23,16 23,18.5C23,21 21,23 18.5,23C16,23 14,21 14,18.5C14,17.62 14.25,16.8 14.69,16.1L14.59,16H9.41L9.31,16.1C9.75,16.8 10,17.62 10,18.5C10,21 8,23 5.5,23C3,23 1,21 1,18.5C1,16 3,14 5.5,14C6.38,14 7.2,14.25 7.9,14.69L9,13.59V10.41L7.9,9.31C7.2,9.75 6.38,10 5.5,10C3,10 1,8 1,5.5C1,3 3,1 5.5,1M5.5,3A2.5,2.5 0 0,0 3,5.5A2.5,2.5 0 0,0 5.5,8A2.5,2.5 0 0,0 8,5.5A2.5,2.5 0 0,0 5.5,3M5.5,16A2.5,2.5 0 0,0 3,18.5A2.5,2.5 0 0,0 5.5,21A2.5,2.5 0 0,0 8,18.5A2.5,2.5 0 0,0 5.5,16M18.5,3A2.5,2.5 0 0,0 16,5.5A2.5,2.5 0 0,0 18.5,8A2.5,2.5 0 0,0 21,5.5A2.5,2.5 0 0,0 18.5,3M18.5,16A2.5,2.5 0 0,0 16,18.5A2.5,2.5 0 0,0 18.5,21A2.5,2.5 0 0,0 21,18.5A2.5,2.5 0 0,0 18.5,16M3.91,17.25L5.04,17.91C5.17,17.81 5.33,17.75 5.5,17.75A0.75,0.75 0 0,1 6.25,18.5L6.24,18.6L7.37,19.25L7.09,19.75L5.96,19.09C5.83,19.19 5.67,19.25 5.5,19.25A0.75,0.75 0 0,1 4.75,18.5L4.76,18.4L3.63,17.75L3.91,17.25M3.63,6.25L4.76,5.6L4.75,5.5A0.75,0.75 0 0,1 5.5,4.75C5.67,4.75 5.83,4.81 5.96,4.91L7.09,4.25L7.37,4.75L6.24,5.4L6.25,5.5A0.75,0.75 0 0,1 5.5,6.25C5.33,6.25 5.17,6.19 5.04,6.09L3.91,6.75L3.63,6.25M16.91,4.25L18.04,4.91C18.17,4.81 18.33,4.75 18.5,4.75A0.75,0.75 0 0,1 19.25,5.5L19.24,5.6L20.37,6.25L20.09,6.75L18.96,6.09C18.83,6.19 18.67,6.25 18.5,6.25A0.75,0.75 0 0,1 17.75,5.5L17.76,5.4L16.63,4.75L16.91,4.25M16.63,19.25L17.75,18.5A0.75,0.75 0 0,1 18.5,17.75C18.67,17.75 18.83,17.81 18.96,17.91L20.09,17.25L20.37,17.75L19.25,18.5A0.75,0.75 0 0,1 18.5,19.25C18.33,19.25 18.17,19.19 18.04,19.09L16.91,19.75L16.63,19.25Z",
    atv: "M20 11C19.8 11 19.6 11 19.5 11.1L17.4 9H20V6L16.3 7.9L13.4 5H9V7H12.6L14.6 9H11L7 11L5 9H0V11H4C1.8 11 0 12.8 0 15S1.8 19 4 19 8 17.2 8 15L10 17H13L16.5 10.9L17.5 11.9C16.6 12.6 16 13.8 16 15C16 17.2 17.8 19 20 19S24 17.2 24 15 22.2 11 20 11M4 17C2.9 17 2 16.1 2 15S2.9 13 4 13 6 13.9 6 15 5.1 17 4 17M20 17C18.9 17 18 16.1 18 15S18.9 13 20 13 22 13.9 22 15 21.1 17 20 17Z",
    tractor: "M5,4V11.26C3.2,11.9 2,13.6 2,15.5C2,18 4,20 6.5,20C8.79,20 10.71,18.28 10.97,16H15.17C15.06,16.32 15,16.66 15,17A3,3 0 0,0 18,20A3,3 0 0,0 21,17C21,16.66 20.94,16.32 20.82,16H22V13C22,11.89 21.11,11 20,11H15.04L13.65,4H5M7,6H12L13,11V14H10.74C10.16,12.38 8.71,11.23 7,11.03V6M6.5,13.25A2.25,2.25 0 0,1 8.75,15.5A2.25,2.25 0 0,1 6.5,17.75A2.25,2.25 0 0,1 4.25,15.5A2.25,2.25 0 0,1 6.5,13.25M18,15.5A1.5,1.5 0 0,1 19.5,17A1.5,1.5 0 0,1 18,18.5A1.5,1.5 0 0,1 16.5,17A1.5,1.5 0 0,1 18,15.5Z",
    excavator: "M18.5 18.5C19.04 18.5 19.5 18.96 19.5 19.5S19.04 20.5 18.5 20.5H6.5C5.96 20.5 5.5 20.04 5.5 19.5S5.96 18.5 6.5 18.5H18.5M18.5 17H6.5C5.13 17 4 18.13 4 19.5S5.13 22 6.5 22H18.5C19.88 22 21 20.88 21 19.5S19.88 17 18.5 17M21 11H18V7H13L10 11V16H22L21 11M11.54 11L13.5 8.5H16V11H11.54M9.76 3.41L4.76 2L2 11.83C1.66 13.11 2.41 14.44 3.7 14.8L4.86 15.12L8.15 12.29L4.27 11.21L6.15 4.46L8.94 5.24C9.5 5.53 10.71 6.34 11.47 7.37L12.5 6H12.94C11.68 4.41 9.85 3.46 9.76 3.41Z",
    loader: "M4,4A1,1 0 0,0 3,5V10C2.54,10 2.14,10.31 2.03,10.76V13.97H2.29C2.65,13.37 3.3,13 4,13H13C13.7,13 14.35,13.37 14.71,13.97H16.03L16,11V11A1,1 0 0,0 15,10H13V8A1,1 0 0,0 12,7A1,1 0 0,0 11,8V10H9V5A1,1 0 0,0 8,4H4M5,6H7V10L7,11H5V6M17,11V19H22V18L19,17L18,11H17M4,15A2,2 0 0,0 2,17A2,2 0 0,0 4,19H13A2,2 0 0,0 15,17A2,2 0 0,0 13,15H4Z",
    forest: "M16 12L9 2L2 12H3.86L0 18H7V22H11V18H18L14.14 12H16M20.14 12H22L15 2L12.61 5.41L17.92 13H15.97L19.19 18H24L20.14 12M13 19H17V22H13V19Z",
    snowmobile: "M20.34 17.93L18.4 16C19.5 15.91 22.66 15.53 22.97 13.36C23 13.12 22.92 12.89 22.78 12.7C22.12 11.82 19.88 9.07 15 5H12V7H14L16 9L12 11L3 10L1 13L5.58 14.38L2.58 16.09C2.22 16.26 1 16.89 1 18.05C1 18.4 1 18.89 1.33 19.32C1.83 20 2.71 20 3 20H9C10.26 20 12.53 19.13 12.93 16H15.59L17.59 18H15V20H20C21.65 20 23 18.65 23 17H21C21 17.43 20.73 17.79 20.34 17.93M9 18H3.25C3.33 17.95 3.41 17.91 3.5 17.87L8.22 15.17L10.92 16C10.58 17.84 9.28 18 9 18Z",
    truck: "M18,18.5A1.5,1.5 0 0,1 16.5,17A1.5,1.5 0 0,1 18,15.5A1.5,1.5 0 0,1 19.5,17A1.5,1.5 0 0,1 18,18.5M19.5,9.5L21.46,12H17V9.5M6,18.5A1.5,1.5 0 0,1 4.5,17A1.5,1.5 0 0,1 6,15.5A1.5,1.5 0 0,1 7.5,17A1.5,1.5 0 0,1 6,18.5M20,8H17V4H3C1.89,4 1,4.89 1,6V17H3A3,3 0 0,0 6,20A3,3 0 0,0 9,17H15A3,3 0 0,0 18,20A3,3 0 0,0 21,17H23V12L20,8Z",
    sand: "M14,6L10.25,11L13.1,14.8L11.5,16C9.81,13.75 7,10 7,10L1,18H23L14,6Z",
    generator: "M6 3C4.89 3 4 3.9 4 5V16H6V17C6 17.55 6.45 18 7 18H8C8.55 18 9 17.55 9 17V16H15V17C15 17.55 15.45 18 16 18H17C17.55 18 18 17.55 18 17V16H20V5C20 3.9 19.11 3 18 3H6M12 7V5H18V7H12M12 9H18V11H12V9M8 5V9H10L7 15V11H5L8 5M22 20V22H2V20H22Z",
    premises: "M0,21V10L7.5,5L15,10V21H10V14H5V21H0M24,2V21H17V8.93L16,8.27V6H14V6.93L10,4.27V2H24M21,14H19V16H21V14M21,10H19V12H21V10M21,6H19V8H21V6Z",
    comms: "M12,10A2,2 0 0,1 14,12C14,12.5 13.82,12.94 13.53,13.29L16.7,22H14.57L12,14.93L9.43,22H7.3L10.47,13.29C10.18,12.94 10,12.5 10,12A2,2 0 0,1 12,10M12,8A4,4 0 0,0 8,12C8,12.5 8.1,13 8.28,13.46L7.4,15.86C6.53,14.81 6,13.47 6,12A6,6 0 0,1 12,6A6,6 0 0,1 18,12C18,13.47 17.47,14.81 16.6,15.86L15.72,13.46C15.9,13 16,12.5 16,12A4,4 0 0,0 12,8M12,4A8,8 0 0,0 4,12C4,14.36 5,16.5 6.64,17.94L5.92,19.94C3.54,18.11 2,15.23 2,12A10,10 0 0,1 12,2A10,10 0 0,1 22,12C22,15.23 20.46,18.11 18.08,19.94L17.36,17.94C19,16.5 20,14.36 20,12A8,8 0 0,0 12,4Z",
    boat: "M3 13.5L11 2.03V13.5H3M12.5 13.5C13.85 9.75 13.67 4.71 12.5 1C17.26 2.54 20.9 8.4 20.96 13.5H12.5M21.1 17.08C20.69 17.72 20.21 18.27 19.65 18.74C19 18.45 18.42 18 17.96 17.5C16.47 19.43 13.46 19.43 11.97 17.5C10.5 19.43 7.47 19.43 6 17.5C5.5 18 4.95 18.45 4.3 18.74C3.16 17.8 2.3 16.46 2 15H21.94C21.78 15.75 21.5 16.44 21.1 17.08M20.96 23C19.9 23 18.9 22.75 17.96 22.25C16.12 23.25 13.81 23.25 11.97 22.25C10.13 23.25 7.82 23.25 6 22.25C4.77 22.94 3.36 23.05 2 23V21C3.41 21.05 4.77 20.9 6 20C7.74 21.25 10.21 21.25 11.97 20C13.74 21.25 16.2 21.25 17.96 20C19.17 20.9 20.54 21.05 21.94 21V23H20.96Z",
    crew: "M12,5.5A3.5,3.5 0 0,1 15.5,9A3.5,3.5 0 0,1 12,12.5A3.5,3.5 0 0,1 8.5,9A3.5,3.5 0 0,1 12,5.5M5,8C5.56,8 6.08,8.15 6.53,8.42C6.38,9.85 6.8,11.27 7.66,12.38C7.16,13.34 6.16,14 5,14A3,3 0 0,1 2,11A3,3 0 0,1 5,8M19,8A3,3 0 0,1 22,11A3,3 0 0,1 19,14C17.84,14 16.84,13.34 16.34,12.38C17.2,11.27 17.62,9.85 17.47,8.42C17.92,8.15 18.44,8 19,8M5.5,18.25C5.5,16.18 8.41,14.5 12,14.5C15.59,14.5 18.5,16.18 18.5,18.25V20H5.5V18.25M0,20V18.5C0,17.11 1.89,15.94 4.45,15.6C3.86,16.28 3.5,17.22 3.5,18.25V20H0M24,20H20.5V18.25C20.5,17.22 20.14,16.28 19.55,15.6C22.11,15.94 24,17.11 24,18.5V20Z",
    transport: "M3,13.5L2.25,12H7.5L6.9,10.5H2L1.25,9H9.05L8.45,7.5H1.11L0.25,6H4A2,2 0 0,1 6,4H18V8H21L24,12V17H22A3,3 0 0,1 19,20A3,3 0 0,1 16,17H12A3,3 0 0,1 9,20A3,3 0 0,1 6,17H4V13.5H3M19,18.5A1.5,1.5 0 0,0 20.5,17A1.5,1.5 0 0,0 19,15.5A1.5,1.5 0 0,0 17.5,17A1.5,1.5 0 0,0 19,18.5M20.5,9.5H18V12H22.46L20.5,9.5M9,18.5A1.5,1.5 0 0,0 10.5,17A1.5,1.5 0 0,0 9,15.5A1.5,1.5 0 0,0 7.5,17A1.5,1.5 0 0,0 9,18.5Z",
    snow: "M20.79,13.95L18.46,14.57L16.46,13.44V10.56L18.46,9.43L20.79,10.05L21.31,8.12L19.54,7.65L20,5.88L18.07,5.36L17.45,7.69L15.45,8.82L13,7.38V5.12L14.71,3.41L13.29,2L12,3.29L10.71,2L9.29,3.41L11,5.12V7.38L8.5,8.82L6.5,7.69L5.92,5.36L4,5.88L4.47,7.65L2.7,8.12L3.22,10.05L5.55,9.43L7.55,10.56V13.45L5.55,14.58L3.22,13.96L2.7,15.89L4.47,16.36L4,18.12L5.93,18.64L6.55,16.31L8.55,15.18L11,16.62V18.88L9.29,20.59L10.71,22L12,20.71L13.29,22L14.7,20.59L13,18.88V16.62L15.5,15.17L17.5,16.3L18.12,18.63L20,18.12L19.53,16.35L21.3,15.88L20.79,13.95M9.5,10.56L12,9.11L14.5,10.56V13.44L12,14.89L9.5,13.44V10.56Z",
    machines: "M21.71 20.29L20.29 21.71A1 1 0 0 1 18.88 21.71L7 9.85A3.81 3.81 0 0 1 6 10A4 4 0 0 1 2.22 4.7L4.76 7.24L5.29 6.71L6.71 5.29L7.24 4.76L4.7 2.22A4 4 0 0 1 10 6A3.81 3.81 0 0 1 9.85 7L21.71 18.88A1 1 0 0 1 21.71 20.29M2.29 18.88A1 1 0 0 0 2.29 20.29L3.71 21.71A1 1 0 0 0 5.12 21.71L10.59 16.25L7.76 13.42M20 2L16 4V6L13.83 8.17L15.83 10.17L18 8H20L22 4Z",
    evac: "M13.34,8.17C12.41,8.17 11.65,7.4 11.65,6.47A1.69,1.69 0 0,1 13.34,4.78C14.28,4.78 15.04,5.54 15.04,6.47C15.04,7.4 14.28,8.17 13.34,8.17M10.3,19.93L4.37,18.75L4.71,17.05L8.86,17.9L10.21,11.04L8.69,11.64V14.5H7V10.54L11.4,8.67L12.07,8.59C12.67,8.59 13.17,8.93 13.5,9.44L14.36,10.79C15.04,12 16.39,12.82 18,12.82V14.5C16.14,14.5 14.44,13.67 13.34,12.4L12.84,14.94L14.61,16.63V23H12.92V17.9L11.14,16.21L10.3,19.93M21,23H19V3H6V16.11L4,15.69V1H21V23M6,23H4V19.78L6,20.2V23Z",
    other: "M10,19H13V22H10V19M12,2C17.35,2.22 19.68,7.62 16.5,11.67C15.67,12.67 14.33,13.33 13.67,14.17C13,15 13,16 13,17H10C10,15.33 10,13.92 10.67,12.92C11.33,11.92 12.67,11.33 13.5,10.67C15.92,8.43 15.32,5.26 12,5A3,3 0 0,0 9,8H6A6,6 0 0,1 12,2Z",
    actor: "M11.5,1L2,6V8H21V6M16,10V17H19V10M2,22H21V19H2M10,10V17H13V10M4,10V17H7V10H4Z"
  };

  // Type (fra Catalog.cs) -> ikon
  var TYPE_GLYPH = {
    'Drone og droneoperatør': 'drone', 'Droneobservasjon': 'drone',
    'ATV': 'atv', 'Traktor': 'tractor', 'Gravemaskin': 'excavator', 'Hjullaster': 'loader',
    'Skogsmaskin': 'forest', 'Snøskuter': 'snowmobile', 'Lastebil': 'truck', 'Sand/grus': 'sand',
    'Aggregat': 'generator', 'Strøm/aggregat': 'generator', 'Lokaler': 'premises',
    'Sambandsutstyr': 'comms', 'Samband': 'comms', 'Båt': 'boat',
    'Mannskap/frivillige': 'crew', 'Mannskap': 'crew', 'Transport': 'transport',
    'Snørydding': 'snow', 'Maskiner': 'machines', 'Evakuering': 'evac',
    'Annet': 'other', 'Offentlig aktør': 'actor'
  };

  var COLORS = { acute: '#c62828', planned: '#f9a825', resource: '#2e7d32', actor: '#1565c0' };
  var SHAPES = { acute: 'triangle', planned: 'diamond', resource: 'circle', actor: 'square' };
  // [skala, midtpunkt-y] for ikonet inni formen
  var GLYPH_FIT = { acute: [0.46, 25], planned: [0.56, 20], resource: [0.62, 20], actor: [0.7, 20] };

  function shapeEl(shape, attrs) {
    switch (shape) {
      case 'circle':   return '<circle cx="20" cy="20" r="13" ' + attrs + '/>';
      case 'square':   return '<rect x="8" y="8" width="24" height="24" rx="5" ' + attrs + '/>';
      case 'diamond':  return '<polygon points="20,6 34,20 20,34 6,20" ' + attrs + '/>';
      default:         return '<polygon points="20,7 34,33 6,33" ' + attrs + '/>';   // triangle
    }
  }

  function glyphFor(type) {
    return GLYPHS[TYPE_GLYPH[type] || 'other'];
  }

  // type = null gir bare formen (brukes i tegnforklaringen)
  function markerSvg(kind, type, size) {
    var shape = SHAPES[kind];
    var s = size || 42;
    var out = '<svg xmlns="http://www.w3.org/2000/svg" width="' + s + '" height="' + s + '" viewBox="0 0 40 40" aria-hidden="true">' +
      shapeEl(shape, 'fill="#000" stroke="#000" stroke-width="9.5" stroke-linejoin="round"') +   // svart ytterkant
      shapeEl(shape, 'fill="#fff" stroke="#fff" stroke-width="5" stroke-linejoin="round"') +     // hvit kant innenfor
      shapeEl(shape, 'fill="' + COLORS[kind] + '"');
    if (type !== null && type !== undefined) {
      var fit = GLYPH_FIT[kind];
      var sc = fit[0];
      var color = kind === 'planned' ? '#1b1b1b' : '#ffffff';
      out += '<path d="' + glyphFor(type) + '" fill="' + color + '" transform="translate(' +
        (20 - 12 * sc).toFixed(2) + ' ' + (fit[1] - 12 * sc).toFixed(2) + ') scale(' + sc + ')"/>';
    }
    return out + '</svg>';
  }

  // Enkelt ikon (svart) for typelisten i tegnforklaringen
  function typeIconSvg(type) {
    return '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" aria-hidden="true">' +
      '<path d="' + glyphFor(type) + '" fill="#111"/></svg>';
  }

  var PICK =
    '<svg xmlns="http://www.w3.org/2000/svg" width="36" height="44" viewBox="0 0 36 44" aria-hidden="true">' +
    '<path d="M18 42C18 42 4 27 4 17a14 14 0 0 1 28 0c0 10-14 25-14 25z" fill="#1565c0" stroke="#000" stroke-width="4"/>' +
    '<path d="M18 42C18 42 4 27 4 17a14 14 0 0 1 28 0c0 10-14 25-14 25z" fill="#1565c0" stroke="#fff" stroke-width="2"/>' +
    '<circle cx="18" cy="17" r="5" fill="#ffffff"/></svg>';

  function icon(kind, type) {
    if (kind === 'pick') {
      return L.divIcon({ className: 'fm-marker', html: PICK, iconSize: [36, 44], iconAnchor: [18, 42], popupAnchor: [0, -38] });
    }
    return L.divIcon({ className: 'fm-marker', html: markerSvg(kind, type), iconSize: [42, 42], iconAnchor: [21, 21], popupAnchor: [0, -20] });
  }

  // Topografisk kart fra Kartverket (åpne data)
  function addBaseLayer(map) {
    return L.tileLayer('https://cache.kartverket.no/v1/wmts/1.0.0/topo/default/webmercator/{z}/{y}/{x}.png', {
      minZoom: 3, maxZoom: 20, maxNativeZoom: 18,
      attribution: '&copy; <a href="https://www.kartverket.no/">Kartverket</a>'
    }).addTo(map);
  }

  // Samlemarkør for ressurser som ligger tett (marker clustering)
  function clusterIcon(count) {
    return L.divIcon({
      className: 'fm-cluster',
      html: '<div class="fm-cluster-inner">' + count + '</div>',
      iconSize: [44, 44]
    });
  }

  function toast(message, type, href, linkText) {
    var box = document.querySelector('.fm-alerts');
    if (!box) return;
    var d = document.createElement('div');
    d.className = 'alert alert-' + (type || 'success') + ' alert-dismissible shadow';
    d.setAttribute('role', type === 'danger' ? 'alert' : 'status');
    d.append(document.createTextNode(message + ' '));
    if (href) {
      var a = document.createElement('a');
      a.href = href;
      a.className = 'alert-link';
      a.textContent = linkText || 'Åpne';
      d.append(a);
    }
    var b = document.createElement('button');
    b.type = 'button';
    b.className = 'btn-close';
    b.setAttribute('data-bs-dismiss', 'alert');
    b.setAttribute('aria-label', 'Lukk');
    d.append(b);
    box.append(d);
    setTimeout(function () { d.remove(); }, 9000);
  }

  return {
    NORWAY: NORWAY, icon: icon, markerSvg: markerSvg, typeIconSvg: typeIconSvg,
    addBaseLayer: addBaseLayer, clusterIcon: clusterIcon, toast: toast
  };
})();
