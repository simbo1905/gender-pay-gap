    $('form').bind('invalid-form.validate', function (e, v) {

        window.setTimeout(function () {
            var list = $(".validation-summary-errors ul");
            if (!list) return;
            list.empty();

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

                $("<li />").html(summary).appendTo(list);
            })

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