(function ($) {
    var methods = {
        hostGame: function (selector) {
            if (!alertify.hostGame) {
                alertify.dialog('hostGame', function () {
                    return {
                        main: function (content) {
                            this.setContent(content);
                            this.setHeader($(content).find('.PartialHostGame_Header')[0]);
                            $(this.elements.footer).append($(content).find('.PartialHostGame_Footer')[0]);
                            return this;
                        },
                        setup: function () {
                            return {
                                options: {
                                    basic: false,
                                    maximizable: false,
                                    resizable: false,
                                    padding: true,
                                },
                                buttons: [
                                    {
                                        text: 'Host',
                                        invokeOnClose: false,
                                    }
                                ]
                            };
                        },
                        hooks: {
                            onshow: function () {
                                this.elements.dialog.style.maxWidth = '200px';
                                this.elements.dialog.style.width = '200px';
                            }
                        }
                    };
                });
            } 
            return alertify.hostGame(selector[0]);
        }
    };

    $.fn.customDialog = function (methodOrOptions) {
        if (methods[methodOrOptions]) {
            var adds = [this];
            adds = adds.concat(Array.prototype.slice.call(arguments, 1));
            return methods[methodOrOptions].apply(this, adds);
        } else {
            $.error('Method ' + methodOrOptions + ' does not exist on jQuery.customDialog');
        }
    };
}(jQuery));