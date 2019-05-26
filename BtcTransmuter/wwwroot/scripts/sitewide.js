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
});