/* global $ */
/* global GOVUK */

// Warn about using the kit in production
if (
  window.sessionStorage && window.sessionStorage.getItem('prototypeWarning') !== 'false' &&
  window.console && window.console.info
  ) {
    window.console.info('GOV.UK Prototype Kit - do not use for production')
    window.sessionStorage.setItem('prototypeWarning', true)
}

$(document).ready(function () {
    // Use GOV.UK selection-buttons.js to set selected
    // and focused states for block labels
    var $blockLabels = $(".block-label input[type='radio'], .block-label input[type='checkbox']")
    new GOVUK.SelectionButtons($blockLabels) // eslint-disable-line

    // Use GOV.UK shim-links-with-button-role.js to trigger a link styled to look like a button,
    // with role="button" when the space key is pressed.
    GOVUK.shimLinksWithButtonRole.init()

    // Show and hide toggled content
    // Where .block-label uses the data-target attribute
    // to toggle hidden content
    var showHideContent = new GOVUK.ShowHideContent()
    showHideContent.init()
})

$(document).ready(function () {
    $('.companyType .submit').click(function (e) {
        if ($('#radio-1, #radio-2').is(':checked')) {
            window.location.replace("organisation-search");
        }
        else if ($('#radio-3').is(':checked')) {
            window.location.replace("private-organisation-search?dc=charity");
        }
        else if ($('#radio-4').is(':checked')) {
            window.location.replace("organisation-search?dc=plc");
        }
        else if ($('#radio-5').is(':checked')) {
            window.location.replace("organisation-search?dc=public");
        }

        else if ($('#radio-6').is(':checked')) {
            window.location.replace("organisation-search?dc=privateManual");
        }

        else if ($('#radio-7').is(':checked')) {
            window.location.replace("organisation-search?dc=publicManual");
        }
    });
})


/*$(document).ready(function () {
  $('.parent .submit').click(function (e) {
    if ($('#radio-1').is(':checked')) {
      window.location.replace("id-relationship?dc=ltd");
    }
    else if ($('#radio-2').is(':checked')) {
      window.location.replace("pin-success?dc=ltd");
    }
    else if ($('#radio-3').is(':checked')) {
      window.location.replace("id-relationship?dc=plc");
    }
    else if ($('#radio-4').is(':checked')) {
      window.location.replace("pin-success?dc=plc");
    }    
  });
})
*/


$(document).ready(function () {

    // Parse the URL parameter
    function getParameterByName(name, url) {
        if (!url) url = window.location.href;
        name = name.replace(/[\[\]]/g, "\\$&");
        var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, " "));
    }
    // Give the parameter a variable name
    var dynamicContent = getParameterByName('dc');
    var public = getParameterByName('dc2');
    var address = getParameterByName('ad');
    var sector = getParameterByName('sec');
    var gender = getParameterByName('gen');
    var signature = getParameterByName('sign');
    var update = getParameterByName('up');

    $(document).ready(function () {

        // Check if the URL parameter is apples
        if (dynamicContent == 'charity') {
            $('.charity').show();
        }
            // Check if the URL parameter is oranges
        else if (dynamicContent == 'public') {
            $('.public').show();
        }
        else if (public == 'public') {
            $('.public').show();
            $('.update-public').show();
        }
            // Check if the URL parameter is bananas
        else if (dynamicContent == 'ltd') {
            $('.ltd').show();
        }
        else if (update == 'up') {
            $('.update').show();
        }
        else if (dynamicContent == 'plc') {
            $('.plc').show();
        }
        else if (dynamicContent == 'manual') {
            $('.manual').show();
        }
        else if (dynamicContent == 'privateManual') {
            $('.privateManual').show();
        }
        else if (dynamicContent == 'publicManual') {
            $('.publicManual').show();
        }
        else if (dynamicContent == 'privateManualReject') {
            $('.privateManualReject').show();
        }
        else if (dynamicContent == 'publicManualReject') {
            $('.publicManualReject').show();
        }


        else if (dynamicContent == 'pin') {
            $('.pin').show();
        }
        else if (dynamicContent == 'alert') {
            $('.alert').show();
        }
        else if (dynamicContent == 'correct') {
            $('.correct').show();
        }

        else if (dynamicContent == 'update') {
            $('.update').show();
        }
        else if (signature == 'false') {
            $('.sign').hide();
        }
            // Check if the URL parameter is empty or not defined, display default content
        else {
            $('.default-content').show();
            $('.sign').show();

        }
        $("#address").html(address);

        $("#companyname").html(dynamicContent);
        $("#sector").html(sector);


        if (gender == 'Women') {
            $(".gender").html('Womens\'');
        }
        else {
            $(".gender").html('Mens\'');
        }


    });
})

$(document).ready(function () {

    $('ul.tabs li').click(function () {
        var tab_id = $(this).attr('data-tab');

        $('ul.tabs li').removeClass('current');
        $('.tab-content').removeClass('current');

        $(this).addClass('current');
        $("#" + tab_id).addClass('current');
    })

})


$('#singular').submit(function () {
    var search = $('#search').val();
    if (search == 'victors') {
        $(this).attr('action', "search-results-single");
    }
    else if (search == 'smith') {
        $(this).attr('action', "search-results-smith");

    }
});

$('#singular').submit(function () {
    var search = $('#search').val();
    if (search == 'victors') {
        $(this).attr('action', "search-results-single");
    }
    else if (search == 'smith') {
        $(this).attr('action', "search-results-smith");

    }
});


$(document).ready(function (getNumber) {

    var minNumber = 4; // The minimum number you want
    var maxNumber = 17; // The maximum number you want
    var randomnumber = Math.floor(Math.random() * (maxNumber + 1) + minNumber); // Generates random number
    $('.percent').html(randomnumber); // Sets content of <div> to number
    $('.percent2').html(randomnumber + 2); // Sets content of <div> to number

    return false; // Returns false just to tidy everything up

})


$("#application").change(function () {
    console.log(this.value);
    $("#link_combo").attr('href', this.value);
});

$(document).ready(function (toggleContent) {




    $('.toggle').each(function (index) {


        // Variables

        var $link = $(this).find('.toggle-link'),
            $content = $(this).find('.toggle-content'),
            $id = 'toggle-' + (index++);


        // Add attributes to toggler link

        $link.attr('aria-controls', $id).attr({
            'aria-expanded': false
        });


        // Add attributes to toggle content

        $content.attr({
            'id': $id,
            'aria-hidden': true,
            'style': 'display: none;'
        });


        // Toggle state

        $link.on('click', function (e) {

            e.preventDefault();

            var state = $(this).attr('aria-expanded') === 'false' ? true : false;

            $link.attr('aria-expanded', state);
            $content.attr('aria-hidden', !state).toggle();

        });


    });


});
