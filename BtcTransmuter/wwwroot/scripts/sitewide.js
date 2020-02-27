$(document).ready(function () {

    $(".autocomplete").each(function () {
        var element = $(this);
        var data = element.data("datasrc");

        element.autocomplete({
            hint: false,
            minLength: 0,
            
        }, [
            {
                source: substringMatcher(data),
                cache: false,
                name: "main",
                displayKey: function(x){
                    return x;
                }
                
            }
        ]);
    });

    function substringMatcher(key) {
        return function findMatches(q, cb) {
            var strs = window[key];
            var matches, substringRegex;

            // an array that will be populated with substring matches
            matches = [];

            // regex used to determine if a string contains the substring `q`
            substrRegex = new RegExp(q, 'i');

            // iterate through the pool of strings and for any string that
            // contains the substring `q`, add it to the `matches` array
            $.each(strs, function (i, str) {
                if (substrRegex.test(str)) {
                    matches.push(str);
                }
            });

            cb(matches);
        };
    }

    $(".localizeDate").each(function (index) {
        var serverDate = $(this).text();
        var localDate = new Date(serverDate);

        var dateString = localDate.toLocaleDateString() + " " + localDate.toLocaleTimeString();
        $(this).text(dateString);
    });

    // intializing date time pickers throughts website
    $(".flatdtpicker").each(function () {
        var element = $(this);
        var fdtp = element.attr("data-fdtp");

        // support for initializing with special options per instance
        if (fdtp) {
            var parsed = JSON.parse(fdtp);
            element.flatpickr(parsed);
        } else {
            var min = element.attr("min");
            var max = element.attr("max");
            var defaultDate = element.attr("value");
            element.flatpickr({
                enableTime: true,
                enableSeconds: true,
                dateFormat: 'Z',
                altInput: true,
                altFormat: 'Y-m-d H:i:S',
                minDate: min,
                maxDate: max,
                defaultDate: defaultDate,
                time_24hr: true,
                defaultHour: 0
            });
        }
    });


    $(".input-group-clear").on("click", function () {
        $(this).parents(".input-group").find("input").val(null);
        handleInputGroupClearButtonDisplay(this);
    });

    $(".input-group-clear").each(function () {
        var inputGroupClearBtn = this;
        handleInputGroupClearButtonDisplay(inputGroupClearBtn);
        $(this).parents(".input-group").find("input").on("change input", function () {
            handleInputGroupClearButtonDisplay(inputGroupClearBtn);
        });
    });


    $(".only-for-js").show();

    $('.richtext.html').on('summernote.init', function () {
        $(this).summernote('codeview.activate');
    });

    $(".richtext").each(function () {
        $(this).summernote({
            height: 300,
            codemirror: {
                mode: 'text/html',
                htmlMode: true,
                lineNumbers: true,
                theme: 'monokai',
                extraKeys: {"Ctrl-Space": "autocomplete"},
            }
        });
    });


    $(".qr-code").each(function () {
        new QRCode($(this).get(0),
            {
                text: $(this).data("url"),
                width: 150,
                height: 150
            });
    });

    function handleInputGroupClearButtonDisplay(element) {
        var inputs = $(element).parents(".input-group").find("input");

        $(element).hide();
        for (var i = 0; i < inputs.length; i++) {
            var el = inputs.get(i);
            if ($(el).val() || el.attributes.value) {
                $(element).show();
                break;
            }
        }
    }
});