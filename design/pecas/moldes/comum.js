// Funções comuns dos moldes: leitura de parâmetros da URL e o símbolo da Calmaria.
function param(nome, padrao) {
  var v = new URLSearchParams(location.search).get(nome);
  return v === null || v === "" ? padrao : v;
}
function esc(s) {
  return String(s).replace(/[&<>"]/g, function (c) { return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" }[c]; });
}
var SIMBOLO = '<svg viewBox="0 0 48 48" aria-hidden="true"><g fill="none" stroke="currentColor" stroke-width="3.5" stroke-linecap="round">' +
  '<path d="M6 14 C10 9 14 9 18 14 S26 19 30 14 S38 9 42 14"/>' +
  '<path d="M6 24 C10 21.5 14 21.5 18 24 S26 26.5 30 24 S38 21.5 42 24"/>' +
  '<path d="M6 34 H42"/></g></svg>';
function logo() { return '<span class="logo">' + SIMBOLO + 'calmaria</span>'; }
var FORMA = {
  c: '<svg viewBox="0 0 16 16"><circle cx="8" cy="8" r="6.6"/></svg>',
  m: '<svg viewBox="0 0 16 16"><rect x="1.6" y="1.6" width="12.8" height="12.8" rx="2"/></svg>',
  i: '<svg viewBox="0 0 16 16"><path d="M8 .9 15.1 8 8 15.1 .9 8z"/></svg>'
};
