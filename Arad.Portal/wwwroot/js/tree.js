$(document).ready(function() {
    //var toggler = document.getElementsByClassName("caret");
    //var i;

    //for (i = 0; i < toggler.length; i++) {
    //    toggler[i].addEventListener("click", function () {
    //        console.log(this);
    //        this.parentElement.querySelector(".nested").classList.toggle("active");
    //        this.classList.toggle("caret-down");
    //    });
    //}


    $(function () {
        $(document).on("click", ".tree .caret", clickCaret);
        $(document).on("change", ".tree input[type='checkbox']", checkboxChanged);
        //$('.tree').find('input[type="checkbox"]').change(checkboxChanged);

        function clickCaret() {
            this.parentElement.querySelector(".nested").classList.toggle("active");
            this.classList.toggle("caret-down");
        }
        //check children
        function checkboxChanged() {
            var $this = $(this),
                checked = $this.prop("checked"),
                container = $this.parent(),
                siblings = container.siblings();
            container.find('input[type="checkbox"]')
                .prop({
                    indeterminate: false,
                    checked: checked
                })
                .siblings('label')
                .removeClass('custom-checked custom-unchecked custom-indeterminate')
                .addClass(checked ? 'custom-checked' : 'custom-unchecked');

            checkSiblings(container, checked);
        }

        function checkSiblings($el, checked) {
          
            var parent = $el.parent().parent(),
                all = true,
                indeterminate = false;
            //console.log(parent);
            $el.siblings().each(function () {
                //console.log($(this).children('input[type="checkbox"]'));
                return all = ($(this).children('input[type="checkbox"]').prop("checked") === checked);
            });
            //console.log($el.siblings());
         
            if (all && checked) {
                parent.children('input[type="checkbox"]')
                    .prop({
                        indeterminate: false,
                        checked: checked
                    })
                    .siblings('label')
                    .removeClass('custom-checked custom-unchecked custom-indeterminate')
                    .addClass(checked ? 'custom-checked' : 'custom-unchecked');

                checkSiblings(parent, checked);
            }
            else if (all && !checked) {
                indeterminate = parent.find('input[type="checkbox"]:checked').length > 0;

                parent.children('input[type="checkbox"]')
                    .prop("checked", checked)
                    .prop("indeterminate", indeterminate)
                    .siblings('label')
                    .removeClass('custom-checked custom-unchecked custom-indeterminate')
                    .addClass(indeterminate ? 'custom-indeterminate' : (checked ? 'custom-checked' : 'custom-unchecked'));

                checkSiblings(parent, checked);
            }
            else {
                //console.log($el.parents("li").children('input[type="checkbox"]'));
                $el.parents("li").children('input[type="checkbox"]')
                    .prop({
                        indeterminate: false,
                        checked: true
                    })
                    .siblings('label')
                    .removeClass('custom-checked custom-unchecked custom-indeterminate')
                    .addClass('custom-indeterminate');
            }
        }
    });
});
