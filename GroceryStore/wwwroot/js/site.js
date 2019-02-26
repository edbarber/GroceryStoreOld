// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Internet Explorer 
var isIE = navigator.userAgent.indexOf("MSIE") != -1 || (!!document.documentMode == true);

// call coresponding functions here
// ------------------------------------------------
$(document).ready(function () {
    $('.selectable-image-info').each(function (index, element) {
        if (isIE) {
            centerSelectableImageInfo(element);
        }

        // add hidden attribute instead of on page itself because elements' width don't
        // get calculated when they are hidden
        $(element).addClass('hidden');
    });

    // use current target as it should pass in the correct element
    $('.selectable-image').on({
        mouseenter: function (event) {
            selectableImageOnHover(event.currentTarget);
        },
        mouseleave: function (event) {
            selectableImageOnExit(event.currentTarget);
        }
    });
});
// ------------------------------------------------

// seperate functionality here
// ---------------------------------------------------------

// odd way of centering an element however this is the only "hack" (margin 0 auto doesn't work correctly on page) way it will work in IE 11 as
// flex boxes aren't fully compatible
function centerSelectableImageInfo(element) {
    var container = $(element).parent();

    var elementWidth = $(element).outerWidth();
    var containerWidth = container.width();

    var marginLeft = (containerWidth / 2 - elementWidth / 2) / containerWidth * 100;

    $(element).css('margin-left', marginLeft.toString() + '%');
}

function selectableImageOnHover(element) {
    var image = $(element).children('img').first();
    var info = $(element).children('.selectable-image-info').first();

    image.css('opacity', 0.5);
    info.removeClass('hidden');
}

function selectableImageOnExit(element) {
    var image = $(element).children('img').first();
    var info = $(element).children('.selectable-image-info').first();

    image.css('opacity', 1);
    info.addClass('hidden');
}
// ---------------------------------------------------------