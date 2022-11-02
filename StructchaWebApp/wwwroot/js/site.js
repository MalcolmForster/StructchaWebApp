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

//making divs appear overtop of eachother like a slideshow/powerpoint. Used for the product showcase side of Structcha
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

//hides or shows the divs which are used to edit projects in dashboard, also alters the button that is used
function toggleProjectEditDiv(div) {
    if (div.includes('show_')) {
        div = div.replace('show_', '');
        var x = document.getElementById('edit_' + div);
        var editButton = document.getElementById('editButton_' + div);
        var hideButton = document.getElementById('hideButton_' + div);
        x.style.display = "block";
        editButton.style.display = "none";
        hideButton.style.display = "inline-block";
    } else if (div.includes('hide_')) {
        div = div.replace('hide_', '');
        var x = document.getElementById('edit_' + div);
        var editButton = document.getElementById('editButton_' + div);
        var hideButton = document.getElementById('hideButton_' + div);
        x.style.display = "none";
        editButton.style.display = "inline-block";
        hideButton.style.display = "none";
    }
    return false;
};


//----------A GENERIC AJAXPOST FUNCTION FOR USE WITH MANY----------------
function ajaxPost(jsonData, handler, returnDiv) {
    $.ajax({
        type: 'post',
        data: jsonData,
        url: handler,
        dataType: "html",
        headers: {
            RequestVerificationToken:
                $('input:hidden[name="__RequestVerificationToken"]').val()
        },
        success: function (result) {
            var $div = $(result);
            var test = $div.find(returnDiv);
            if ($div.find(returnDiv).length != 0) {
                var divHTML = $div.find(returnDiv).html();
                $(returnDiv).html(divHTML);
            } else {
                $(returnDiv).html(result);
            }            
        }
    });
};

//-----------A GENERIC AJAXGET FUNCTION-----------------------
function ajaxGet(handler, returnDiv) {
    $.ajax({
        type: 'get',
        dataType: 'html',
        contentType: 'application/html; charset=utf-8',
        url: handler,
        success: function (result) {
            $(returnDiv).html(result);
        }
    });
};

function UpdateTaskBar(loop) {
    if ($('#taskBar').length) {       
        ajaxGet("/Index?handler=TaskBarSectionPosts", '#postList');        
        ajaxGet("/Index?handler=TaskBarSectionTasks", '#assignedTasks');        
        ajaxGet("/Index?handler=TaskBarSectionSelf", '#selfTasks');        
        if (loop) {
            //setTimeout(function () { UpdateTaskBar(true); }, 10000);
        }        
    }
};

// Changes the index's mainView div to the new post or task partial ready for user input
function changeMainDisplay(i) {  
    var handler = "";
    if (i == 0) {
        handler = '/?handler=NewProjectPost';
    } else if (i == 1) {
        handler = '/?handler=NewProjectTask';
    }

    ajaxGet(handler, "#mainView");    
    //$.ajax({
    //    type: 'get',
    //    dataType: 'html',
    //    contentType: 'application/html; charset=utf-8',
    //    url: handler,      
    //    success: function (result) {
    //        $("#mainView").html(result);
    //    }
    //});
};

// When adding a new task, this updates the user and role select boxes to match the selected project users and roles which have access to the project
function updateNewTaskAccessSelectors() {
    var selectedProject = $("#TaskProjectCode :selected").val();
    var handler = '/Index?handler=SendSelected&code=' + selectedProject;
    ajaxGet(handler, "#dynamicTaskSelectors");
    //$.ajax({
    //    type: 'get',
    //    dataType: 'html',
    //    contentType: 'application/html; charset=utf-8',
    //    url: handler,
    //    success: function (result) {
    //        $("#dynamicTaskSelectors").html(result);
    //    }
    //});
};

// Changes the index's mainView to view a post or task
function ShowIndexPartial(Id, t) {
    if (t == 0) {        
        handler = '/Index?handler=PostPartial&postId=' + Id;
    } else if (t == 1) {
        handler = '/Index?handler=TaskPartial&taskId=' + Id;
    }
    ajaxGet(handler, "#mainView");

    //Find a better way to do this
    ajaxGet("/Index?handler=TaskBarSectionPosts", '#postList');
    ajaxGet("/Index?handler=TaskBarSectionTasks", '#assignedTasks');
    ajaxGet("/Index?handler=TaskBarSectionSelf", '#selfTasks');

    //$.ajax({
    //    type: 'get',
    //    dataType: 'html',
    //    contentType: 'application/html; charset=utf-8',
    //    url: (handler + Id),
    //    success: function (result) {
    //        $("#mainView").html(result);
    //    }
    //});
};

// method to hide and show divs
function toggleDisplay(item) {
    var displayed = $(item).css('display');
    if (displayed == 'none') {
        $(item).show();
    } else {
        $(item).hide();
    }
};

// show/hide hints using inline onclick command
function ShowHint(obj) {
    var label = '#'+$(obj).val();
    var item = $(label);
    toggleDisplay(item);
};

// show/hide further advanced task assignment div, used to add/block individual users which are in the roles selected
function ToggleAdvancedTaskAssignment() {
    var item = $('#AdvancedTaskAssign');
    toggleDisplay(item);
};

// updates the AdvancedTaskAssign div to show which users and roles have been added to the task, not used anymore?
function updateNewTaskInfo() {
    $.ajax({
        type: 'get',
        dataType: 'html',
        contentType: 'application/html; charset=utf-8',
        url: "/Index?handler=NewProjectTask",
        success: function (result) {
            var newDiv = $(result).find("#AdvancedTaskAssign").html();
            $("#AdvancedTaskAssign").html(newDiv);
        }
    });
};

//var t = $("input[name='__RequestVerificationToken']").val(); //there is a better method where the data is passed from the page view I believe, check bookmarked stoackoverflow page

// remove user from task
function deleteTaskUser(num, name) {
    var list = "";
    if (num == 0) { //remove user from the AssignedUsersList
        list = "_AssignUsers";
    } else if (num == 1) {
        list = "_AssignRoles";
    }
    document.getElementById(name+list).remove();
};

// add user to task
function addTaskAddUser() {
    var userToAdd = $("#TaskAssignUser").val();
    if (userToAdd != "") {
        $('#AssignedUsersList').append('<li id="' + userToAdd + '_AssignUsers">' + userToAdd + '<button style="float:right" type="button" onclick="deleteTaskUser(0,\'' + userToAdd + '\')">Remove</button><input name="userAssigned" type="hidden" value="' + userToAdd +'"/></li>');
    }
};

// add role to a task
function addTaskAddRole() {

    var roleToAdd = $("#TaskAssignRole").val();

    if (roleToAdd != "") {
        $('#AssignedRolesList').append('<li id="' + roleToAdd + '_AssignRoles">' + roleToAdd + '<button style="float:right" type="button" onclick="deleteTaskUser(1,\'' + roleToAdd + '\')">Remove</button><input name="roleAssigned" type="hidden" value="'+roleToAdd+'"/></li>');
    }
};

// block role user from task WIP
function blockTaskRoleUser() {

    updateNewTaskInfo();
};

// unblock role user from task WIP
function unblockTaskRoleUser() {

    updateNewTaskInfo();
};

// Dont think its used anymore, been replaced with ajax methods
//function showUsersRoles(formEle) {
//    formEle.submit();
//}


//uploads images to the server when user adds them to post
$(document).on('change', '#imageSelect', function updateUserImages(event) {
    event.preventDefault;
    var currentImages = $('div[name="uploadedImage"]');
    var formData = new FormData();
    var images = this.files;
    var handler = "/Index?handler=ImageUpload";
    var returnDiv = "#imageUpload";

    for (var child of currentImages) {
        var imageInfo = $(child).find(':input[name="imageInfo"]')[0];
        var imageNum = imageInfo.value;
        var imageId = imageInfo.id;
        var imageLabel = $(child).find(':input[name="draftImageLabel"]')[0].value;
        var imageDescription = $(child).find(' textarea[name="draftImageDescription"]')[0].value;

        var currentImageData = {
            "Number" : imageNum,
            "id" : imageId,
            "Label" : imageLabel,
            "Description" : imageDescription
        };

        //console.log(imageNum + ' ' + imageId + ' ' + imageLabel + ' ' + imageDescription);
        formData.append('CurrentImages', JSON.stringify(currentImageData));
        //console.log(child.$('input[name="imageInfo"]'));
    }

    for (var image of images) {
        formData.append('file', image);
    }

    //ajaxPost(formData, handler, returnDiv);
    //images.forEach(element => console.log(element));

    $.ajax({
        type: 'post',
        data: formData,
        url: handler,
        contentType: false,
        processData: false,
        dataType: "html",
        headers: {
            RequestVerificationToken:
                $('input:hidden[name="__RequestVerificationToken"]').val()
        },
        success: function (result) {
            var $div = $(result);
            var test = $div.find(returnDiv);
            if ($div.find(returnDiv).length != 0) {
                var divHTML = $div.find(returnDiv).html();
                $(returnDiv).html(divHTML);
            } else {
                $(returnDiv).html(result);
            }
        }
    });
});


//displays the current roles of the selected user, and the remove and add role abilities
$(document).on('change', '#userSelect', function showUsersRoles(event) {
    event.preventDefault;
    var userid = $("#userSelect").val();
    var handler = "/Dashboard?handler=FindUser";
    var div = "#roleEditingDiv";
    var jsonData = { user: userid };
    ajaxPost(jsonData, handler, div);
});

//handles adding role to a user with the use of ajaxPost method.
$(document).on('click', '#addUserRole', function addUserRole(event) {
    event.preventDefault;
    var user = $("#userBeingAltered").val();
    var role = $("#userNewRole").val();
    var jsonData = { userBeingAltered: user, newUserRole: role };
    var handler = "/Dashboard?handler=UserRoleAdd";
    var div = "#roleEditingDiv";
    ajaxPost(jsonData, handler, div);
});

//handles deleting role from a user with the use of ajaxPost method.
$(document).on('click', '#deleteUserRole', function (event) {
    event.preventDefault;
    var user = $("#userBeingAltered").val();
    var role = $("#deleteUserRole").val();
    var jsonData = { userBeingAltered: user, newUserRole: role };
    var handler = "/Dashboard?handler=UserRoleRemove";
    var div = "#roleEditingDiv";
    ajaxPost(jsonData, handler, div);
});

//handles the buttons which are used in the post editing forms with ajax
$(document).on('click', '#projectEditButton', function (event) {
    event.preventDefault;    
    var value = $(this).val();
    const splitValue = value.split('_');
    var editDivId = "#edit_" + splitValue[1];
    var theDiv = document.getElementById("edit_" + splitValue[1])
    theDiv.style.display = "block";
    var theSelect = $("#" + value)
    var para = theSelect.find(':selected').val(); 
    var jsonData = { pressedButton : value, parameters : para };
    var handler = "/Dashboard?handler=EditCurrentProject";

    ajaxPost(jsonData, handler, editDivId);    
});

// marks task as done or incompleted
$(document).on('click', '#CompletionButton', function (event) {
    event.preventDefault;
    var SetTo = $("#SetTo").val();
    var taskId = $("#CompletionButton").val();
    var jsonData = { setTo: SetTo, taskId: taskId };
    var handler = "/Index?handler=TaskComplete";
    var mainDiv = "#mainView";

    ajaxPost(jsonData, handler, mainDiv);
    UpdateTaskBarLink(taskId);
    // ajaxGet() //going to update taskbar colours
});

// adds replies to posts and tasks using ajax
function replyToPostOrTask(i) {
    var ReplyBody = $("#ReplyBody").val();
    var id = $("#ReplyTo").val();
    if (i == 0) {
        var handler = "/Index?handler=PostReply";
    } else if (i == 1) {
        var handler = "/Index?handler=TaskReply";
    }    
    var mainDiv = "#mainView";
    var jsonData = { replyBody: ReplyBody, Id: id };

    ajaxPost(jsonData, handler, mainDiv);    
};

//Detects if user wants to post reply to a post
$(document).on('click', '#SubmitReplyPost', function (event) {
    event.preventDefault;
    replyToPostOrTask(0);
});

//Detects if user wants to post reply to a task
$(document).on('click', '#SubmitReplyTask', function (event) {
    event.preventDefault;
    replyToPostOrTask(1);
});

//detects if the webpage has the taskBar div showing
$(window).on('load', function () {
    if ($('#taskBar').length) {
        UpdateTaskBar(true);
    }
});
