$(document).ready(function () {
    flagSideNav= true;
    $('.navbar-toggler').on('click', function(event) {
        event.preventDefault(); 
        if(flagSideNav== false)
        {   
            openNav();
            flagSideNav = !flagSideNav;
        }
        else 
        {
            closeNav();
            flagSideNav = !flagSideNav;
        }
    });

    //sideNav
    //ps = new PerfectScrollbar('.sidebar-nav', {
    //    useScrollbar: false,
    //    // wheelSpeed: 100,
    //    wheelPropagation: true,
    //    minScrollbarLength: 50
    //});

  /*  ee = $('.nav-item .nav-link.active .expand-state').addClass('up');*/
    $(document).on('show.bs.collapse', '.sub.collapse', function (e) {

        elem = e.target;
        el = $(elem).parent().children("a.nav-link").children(".expand-state ");
        //$(el).removeClass('down');
        //$(el).addClass('up');
        /*ps.update();*/
    });
    $(document).on('hide.bs.collapse', '.sub.collapse', function (e) {

        elem = e.target;
        el = $(elem).parent().children("a.nav-link").children(".expand-state ");
        //$(el).removeClass('up');
        //$(el).addClass('down');
        //ps.update();
    });
});
/* Set the width of the side navigation to 250px */
function openNav() {
      /*  $('.app-body').addClass('sidebar-fixed');*/
}

/* Set the width of the side navigation to 0 */
function closeNav() {
      /*  $('.app-body').removeClass('sidebar-fixed');*/
}