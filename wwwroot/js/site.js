// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Selector de tipo de proveedor.
// Muestra los tipos predefinidos y solo habilita el campo de texto libre cuando el
// usuario elige "Otro".
(function () {
  "use strict";

  function initSupplierType() {
    var select = document.getElementById("supplierType");
    var custom = document.getElementById("supplierTypeCustom");
    if (!select || !custom) return;

    function sync(moveFocus) {
      var isOther = select.value === "__otro__";

      if (isOther) {
        select.removeAttribute("name");
        custom.setAttribute("name", "Type");
        custom.classList.remove("d-none");
        if (moveFocus) custom.focus();
      } else {
        select.setAttribute("name", "Type");
        custom.removeAttribute("name");
        custom.classList.add("d-none");
        custom.value = "";
      }
    }

    select.addEventListener("change", function () { sync(true); });
    sync(false);
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initSupplierType);
  } else {
    initSupplierType();
  }
})();
