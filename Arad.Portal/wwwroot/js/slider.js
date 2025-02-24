/*!
 *  Slider.js
 */
$(document).ready(function () {
    //$('.mobile-product-slider').owlCarousel({
    //    rtl:true,
    //    items: 1
    //});

    $('.addresses-slider').owlCarousel({
        loop:false,
        autoplay:false,
        autoplaySpeed: 10000,
        margin: 0,
        nav:true,
        dots:true,
        rtl:true,
        navText: ['<img src="../../img/slider/arrow.png">','<img src="../../img/slider/arrow.png">'],
        responsive:{
            0:{
                items:1,
                stagePadding: 30,
                rtl:true,
                nav:false,
            },
            767:{
                items:1,
                stagePadding: 30,
            },
            991:{
                items:1,
                stagePadding: 30,
            },
            
            1200:{
                items:1,
                stagePadding: 30,
            }
        }
    });

    /*top slider*/
    $('.topSlider').owlCarousel({
        rtl: true,
        center: true,
        loop: true,
        margin: 0,
        nav: false,
        dots: false,
        autoplay: true,
        responsive: {
            0: {
                items: 1
            },
            600: {
                items: 1
            },
            1000: {
                items: 1
            }
        }
    });
})