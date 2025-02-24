////(function($) {

////	"use strict";

////	var carousel = function () {
		
////		$('.owl-carousel').owlCarousel({
////			loop: true,
////            margin: 10,
////			nav: true,
////			items: 1,
////			navText: ["<i class='fas fa-chevron-left'></i>", "<i class='fas fa-chevron-right'></i>"],
////            autoplay: true,
////            responsive: {
////                0: {
////                    items: 1
////                },
////                600: {
////                    items: 3
////                },
////                1000: {
////                    items: 5
////                }
////            }
////	    //loop: true,
////	    //autoplay: true,
////	    //margin:30,
////	    //animateOut: 'fadeOut',
////	    //animateIn: 'fadeIn',
////	    //nav:true,
////	    //dots: true,
////	    //autoplayHoverPause: false,
////	    //items: 1,
////	    //navText: ["<i class='fas fa-chevron-left'></i>", "<i class='fas fa-chevron-right'></i>"],
////	    //responsive:{
////	    //  0:{
////	    //    items:1
////	    //  },
////	    //  600:{
////	    //    items:2
////	    //  },
////	    //  1000:{
////	    //    items:3
////	    //  }
////	    //}
////		});

////	};
////	carousel();

////})(jQuery);

$(document).ready(function () {
	debugger;
	$('.owl-carousel').owlCarousel({
		//loop: true,
		//margin: 10,
		//nav: true,
		//items: 1,
		//navText: ["<i class='fas fa-chevron-left'></i>", "<i class='fas fa-chevron-right'></i>"],
		//autoplay: true,
		//responsive: {
		//	0: {
		//		items: 1
		//	},
		//	600: {
		//		items: 3
		//	},
		//	1000: {
		//		items: 5
		//	}
		//}
		loop: true,
		margin: 10,
		nav: true,
		autoPlay: 1000,
		items: 10,
		responsive: {
			0: {
				items: 1
			},
			600: {
				items: 3
			},
			1000: {
				items: 10
			}
		}
	});
})