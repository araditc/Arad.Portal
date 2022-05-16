$('document').ready(function(){
    allPlus = $('.plus');
    allMinus = $('.minus');
    allConterInputs = $('.count input'),
    priceAll = 0;
    befValInput = 0;
    $('.minus i').css('color','#adacac');

    //all info data attribute of items after load page
    allItems = $('.table-body .table-tr');
    allItemsInfo = [];
    $.each(allItems, function(index, value) {
        rowId = index+1;
        var countItem = $('.table-tr:nth-child('+rowId+')').find('.counter .count input').val();
        var priceOne = $('.table-tr:nth-child('+rowId+')').find('.price-oneItem').data('pon');
        var priceOldAll = $('.table-tr:nth-child('+rowId+')').find('.old-price .price span:first-child').data('pol');
        var discountOne = $('.table-tr:nth-child('+rowId+')').find('.discount .price span:first-child').data('pd');
        var pricePay = $('.table-tr:nth-child('+rowId+')').find('.price-all .price span:first-child').data('pa');
        allItemsInfo[index]={'key':index,'countItem':countItem,'priceOne':priceOne,'priceOldAll':priceOldAll,'discountOne':discountOne,'pricePay':pricePay}
        console.log(value);
        console.log(allItemsInfo[index]);
    }); 


    allConterInputs.keyup(function(e){
       

        if((e.keyCode >= 48) & (e.keyCode <= 57))
        {
            befValInput = $(this).val();
            // console.log(befValInput);
            
        }
        else if(e.keyCode = 8){
            $(this).val('');
            befValInput = $(this).val();
        }
        else{
            $(this).val(befValInput);
            // console.log(e.keyCode);
        }
        // console.log(befValInput.length);
        that = $(this);
        calculatePrice(befValInput,that);
    });
    allConterInputs.blur(function(e){
        if((befValInput.length == 0) || (befValInput == '0')){
            that = $(this);
            $(this).val('1');
            calculatePrice(1,that);
            console.log('blur');
        }
    })
    allPlus.click(function(){
        countElem = $(this).closest('.counter').find('.count input');
        countString = countElem.val();
        count = parseInt(countString);
        that = $(this);
        if(count == 0){
            $(this).closest('.counter').find('.minus i').css('color','rgb(51, 203, 186)');
        }
        count = count+1;
        countElem.val(count);
        befValInput = countElem.val();
        /*increase count  -> increase price */
        calculatePrice(count,that);
    });

    allMinus.click(function(){
        that = $(this);
        countElem = $(this).closest('.counter').find('.count input');
        countString = countElem.val();
        // console.log(countString);
        count = parseInt(countString);
        if(count == 1){
            $(this).closest('.counter').find('.minus i').css('color','#adacac');
            return;//can not select 0
        }
        if(count == 0){
            $(this).closest('.counter').find('.minus i').css('color','#adacac');
            return;
        }
        // console.log(count);
        count = count-1;
        countElem.val(count);
        befValInput = countElem.val();
        /*decrease count  -> decrease price */
        calculatePrice(count,that);
    });

    function calculatePrice(count,that){
        var priceOne= that.closest('.table-tr').find('.price-item-one span.price-one .price-oneItem').data('pon');
        var priceOld= that.closest('.table-tr').find('.price-item-count .old-price .price span:first-child').data('pol');
        var discountAll= that.closest('.table-tr').find('.price-item-count .discount .price span:first-child').data('pd');
        var priceAll= that.closest('.table-tr').find('.price-item-count .price-all .price span:first-child').data('pa');

        priceOld = count*priceOne;
        that.closest('.table-tr').find('.price-item-count .old-price .price span:first-child').html(priceOld);
        discountAll = count*discountAll;
        that.closest('.table-tr').find('.price-item-count .discount .price span:first-child').html(discountAll);
        priceAll = priceOld-discountAll;
        that.closest('.table-tr').find('.price-item-count .price-all .price span:first-child').html(priceAll);

        update(that,priceOld,discountAll,priceAll);
        calculatePriceEndFactor();
    };

    function update(index,priceOld,discountAll,priceAll){
        index = that.closest('.table-tr').index();
        allItemsInfo[index].countItem = befValInput;
        allItemsInfo[index].priceOldAll = priceOld;
        allItemsInfo[index].discountOne = discountAll;
        allItemsInfo[index].pricePay = priceAll;
        console.log(allItemsInfo[index]);
    }
    function calculatePriceEndFactor(){
        var pay = 0;
        var discountFactor = 0;
        var priceFactor = 0;
        allItemsInfo.forEach(element => {
            pay += element.pricePay;
            discountFactor += element.discountOne;
            priceFactor += element.priceOldAll;
        });
        $('.price-basket-shop .pay .price span:first-child').html(pay);
        $('.price-basket-shop .discount .price span:first-child').html(discountFactor);
        $('.price-basket-shop .price-factor .price span:first-child').html(priceFactor);
    }
})   