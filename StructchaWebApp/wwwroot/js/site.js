// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.



/*This method is used when a software (such as Joint Draw) is started from the webapp.
 *  0. Might need to check if the app is already loaded?
 *  
 *  TODO: Add columns to DB to say if webapp has made connection and if localapp has too?
 *  
    1. Refresh div, replacing it with loading symbol, create alert when closing tab stating that "If this tab is closed, app connection will be lost"
    2. Initilise a new app object in C# which creates a session ID etc
    3. Periodically check database using C# to see if sessionID exists in database
    4. When sessionID exists, start the local app and pass info such as user, company, app requested and sessionID information to it.
    5. Change div to say that the app is started
    6. Check every minute or so to see if the app is still connected
*/

$(window).on("load", function () {
    var navHeight = $('#header').height();
    $(window).scroll(function () {
        var windowBottom = $(this).scrollTop() + $(this).innerHeight();
        $(".fadeIn").each(function () {
            $(this).css("top", navHeight);
            $(this).css("height", window.innerHeight - navHeight);
            /* Check the location of each desired element */
            var objectBottom = $(this).offset().top + $(this).outerHeight();
            var objectTop = $(this).offset().top - $(window).scrollTop();
            //console.log($(this).attr('name')); 
            /* If the element is completely within bounds of the window, fade it in */
            //if (objectBottom < windowBottom) { //object comes into view (scrolling down)
            if (objectTop == navHeight) {
                if ($(this).css("opacity") == 0) { $(this).fadeTo(500, 1); }
            } else { //object goes out of view (scrolling up)
                if ($(this).css("opacity") == 1) { $(this).fadeTo(0, 0); }
            }
        });
    }).scroll(); //invoke scroll-handler on page-load
});



