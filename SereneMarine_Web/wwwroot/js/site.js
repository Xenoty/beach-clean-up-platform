// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function dropdown(id, arrowId) {
    var elm = document.getElementById(id).style.display;

    if (elm == "none") {
        document.getElementById(id).style.display = "block";
        document.getElementById(arrowId).innerHTML = "&#8593;";
        map.resize();
    } else {
        document.getElementById(id).style.display = "none";
        document.getElementById(arrowId).innerHTML = "&#8595;";
    }
}