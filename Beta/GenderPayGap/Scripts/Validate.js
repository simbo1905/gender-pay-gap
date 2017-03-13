
    $('form').bind('invalid-form.validate', function (e, v) {

        window.setTimeout(function () {
            var errorsummary=$("[data-valmsg-summary]");
            var list = $("[data-valmsg-summary] ul");
            if (!list) return;

            $(v.errorList).each(function (index, error) {
                //Get the inout element
                var element$ = $(error.element)[0];
                if (!element$) return;
                
                //Get the attribute for this error
                var attrName = GetErrorAttrName(element$, error.message);
                if (!attrName) return;

                var summary = error.message;

                //Get the alternative attribute for this error
                var alt = $(element$).attr(attrName + "-alt");
                if (alt && alt != "") summary = alt;
                
                var item = GetSummaryItem(list, error.message);
                if (item)
                    $(item).html(summary);
                else
                    $(list).append("<li>" + summary + "</li>");
            });

            //Remove duplicates
            DeDupe(list);

            //Show/Hide the summary
            errorsummary.removeClass("customvalidation-summary-errors");
            errorsummary.removeClass("customvalidation-summary-valid");

            if ($(list).children().length > 0)
                errorsummary.addClass("customvalidation-summary-errors");
            else
                errorsummary.addClass("customvalidation-summary-valid");
        }, 100);

       
        //Dont post back
        return false;
    });

    function GetErrorAttrName(element$, error) {
        var result = null;
        $(element$.attributes).each(function (i, attr) {
            if (attr.value == error) {
                result = attr.name;
                return;
            }
        });
        return result;
    }

    function GetSummaryItem(list, error) {
        var result = null;
        $(list).children().each(function (i, item) {
            if ($(item).text() == error) {
                return item;
            }
        });
        return null;
    }

    function DeDupe(list) {
        var seen = {};
        $(list).children().each(function(i, item) {
            var txt = $(item).text();
            if (seen[txt])
                $(item).remove();
            else
                seen[txt] = true;
        });
        return false;
    }

 