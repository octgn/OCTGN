alertify.genericDialog || alertify.dialog('genericDialog', function () {
    return {
        main: function (content, width, onClose) {
            this.setContent(content);
            this.setHeader($(content).find('.GenericDialog_Header')[0]);
            this.settings.width = width;
            this.settings.onClose = onClose;
            $(this.elements.footer).append($(content).find('.GenericDialog_Footer')[0]);
            return this;
        },
        setup: function () {
            return {
                focus: {
                    element: function () {
                        return this.elements.body.querySelector(this.get('selector'));
                    },
                    select: true
                },
                options: {
                    basic:false,
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
        callback:function(closeEvent){
            // index: The index of the button triggering the event.
            // button: The button definition object.
            // cancel: When set true, prevent the dialog from closing.
            onClose(closeEvent);
        },
        build: function () {
        },
        settings: {
            selector: undefined
        },
        hooks: {
            onshow: function() {
                this.elements.dialog.style.maxWidth = this.settings.width;
                this.elements.dialog.style.width = this.settings.width;
            }
        }
    };
});

(function( $ ) {
    $.fn.dialog = function (options) {
        var settings = $.extend({
            // These are the defaults.
        }, options);

        return this;
    };
}(jQuery));