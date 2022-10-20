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
}

function changeMainDisplay(i) {  

    var handler = "";
    if (i == 0) {
        handler = '/?handler=NewProjectPost';
    } else if (i == 1) {
        handler = '/?handler=NewProjectTask';
    }
    
    $.ajax({
        type: 'get',
        dataType: 'html',
        contentType: 'application/html; charset=utf-8',
        url: handler,      
        success: function (result) {
            $("#mainView").html(result);
        }
    });
}




function updateNewTaskAccessSelectors() {
    //$("#TaskAssignUser").empty();
    //$("#TaskAssignRole").empty();

    var selectedProject = $("#TaskProjectCode :selected").val();
    var turl = '/Index?handler=SendSelected&code=' + selectedProject;

    $.ajax({
        type: 'get',
        dataType: 'html',
        contentType: 'application/html; charset=utf-8',
        url: turl,
        success: function (result) {
            $("#dynamicTaskSelectors").html(result);
        }
    });
};


function ShowIndexPartial(Id, t) {
    if (t == 0) {
        handler = '/Index?handler=PostPartial&postId=';
    } else if (t == 1) {
        handler = '/Index?handler=TaskPartial&taskId=';
    }

    $.ajax({
        type: 'get',
        dataType: 'html',
        contentType: 'application/html; charset=utf-8',
        url: (handler + Id),
        success: function (result) {
            $("#mainView").html(result);
        }
    });
};

//$(document).ready(function () {
//    document.ReplyForm.submit('submit', function (e) {
//        console.log("Working");
//        //$.ajax({
//        //    type: 'post',
//        //    dataType: 'json',
//        //    data: $('#ReplyForm').serialize(),
//        //    url: '/Index?handler=NewReply',
//        //    contentType: "application/json; charset=utf-8",
//        //});
//    });
//});

//function ChangeAppRoles(handler, json, returnDiv)
//{
//    $.ajax({
//        type: 'post',
//        data: json,
//        dataType: 'json',
//        url: (handler),
//        success: function (result) {
//            $(returnDiv).html(result);
//        }
//    });
//}

//function AddSoftwareToRole() {
//    handler = "/Index?handler=AddAppRole";
//    div = "#companyAdminTool";
//    software = $(this).val();
//    selectID = software + "_AddRole";
//    roleSelected = $('select[id = ' + selectID + '] option').filter(':selected').val();
//    json = {
//        "app": software,
//        "role": roleSelected
//    }
//    ChangeAppRoles(handler, div);
//}

//function RemoveSoftwareFromRole() {
//    handler = "/Index?handler=RemoveAppRole";
//    div = "#companyAdminTool";
//    software = $(this).val();
//    selectID = software + "_RmvRole";
//    roleSelected = $('select[id = ' + selectID + '] option').filter(':selected').val();
//    json = {
//        "app": software,
//        "role": roleSelected
//    }
//    ChangeAppRoles(handler, json, div);
//}

function toggleDisplay(item) {
    var displayed = $(item).css('display');
    if (displayed == 'none') {
        $(item).show('fast');
    } else {
        $(item).hide('fast');
    }
}

function ShowHint(obj) {
    var label = '#'+$(obj).val();
    var item = $(label);
    toggleDisplay(item);
}

function ToggleAdvancedTaskAssignment() {
    var item = $('#AdvancedTaskAssign');
    toggleDisplay(item);
}

function updateNewTaskInfo() {
    //$("#AdvancedTaskAssign").load("/Pages/Shared/_NewProjectTask.cshtml #AdvancedTaskAssign");

    $.ajax({
        type: 'get',
        dataType: 'html',
        contentType: 'application/html; charset=utf-8',
        url: "/Index?handler=NewProjectTask",
        success: function (result) {
            var newDiv = $(result).find("#AdvancedTaskAssign").html(); 
            //var newDiv = result.getElementById("#AdvancedTaskAssign");
            $("#AdvancedTaskAssign").html(newDiv);
        }
    });
}

var t = $("input[name='__RequestVerificationToken']").val(); //there is a better method where the data is passed from the page view I believe, check bookmarked stoackoverflow page

function deleteTaskUser(num, name) {
    var list = "";
    if (num == 0) { //remove user from the AssignedUsersList
        list = "_AssignUsers";
    } else if (num == 1) {
        list = "_AssignRoles";
    }


    document.getElementById(name+list).remove();

    //listElement.parentNode.removeChild(name);

    //$(list).remove('#'+name);
}

function addTaskAddUser() {
    //$.ajax({
    //    type: 'post',
    //    headers:
    //    {
    //        "RequestVerificationToken": t
    //    },
    //    dataType: 'json',
    //    data: { test: "Another test user yo" },
    //    url: "/Index?handler=NewTaskAddUser",
    //    success: updateNewTaskInfo(),
    //    error: function (responseText, textStatus, XMLHttpRequest) {
    //        return;
    //    }
    //});

    //updateNewTaskInfo();


    //turned into a method of staying client side

    var userToAdd = $("#TaskAssignUser").val();

    if (userToAdd != "") {
        $('#AssignedUsersList').append('<li id="' + userToAdd + '_AssignUsers">' + userToAdd + '<button style="float:right" type="button" onclick="deleteTaskUser(0,\'' + userToAdd + '\')">Remove</button><input name="userAssigned" type="hidden" value="' + userToAdd +'"/></li>');
    }
}

function addTaskAddRole() {

    var roleToAdd = $("#TaskAssignRole").val();

    if (roleToAdd != "") {
        $('#AssignedRolesList').append('<li id="' + roleToAdd + '_AssignRoles">' + roleToAdd + '<button style="float:right" type="button" onclick="deleteTaskUser(1,\'' + roleToAdd + '\')">Remove</button><input name="roleAssigned" type="hidden" value="'+roleToAdd+'"/></li>');
    }
}

function blockTaskRoleUser() {


    updateNewTaskInfo();
}

function unblockTaskRoleUser() {


    updateNewTaskInfo();
}


function showUsersRoles(formEle) {

    formEle.submit();
    //var userselected = document.getElementById("userSelect").value;
    //var userName = JSON.stringify({ username : userselected });

    //$.ajax({
    //    type: 'post',
    //    dataType: 'json',
    //    data: userName,
    //    contentType: 'application/html; charset=utf-8',
    //    url: "/Dashboard?handler=FindUser",
    //});

}

function ajaxPost(jsonData, handler, returnDiv) {

    $.ajax({
        type: 'post',
        //contentType: 'application/json; charset=utf-8',
        //dataType: 'json',
        data: jsonData,
        url: handler,
        dataType: "html",
        headers: {
            RequestVerificationToken:
                $('input:hidden[name="__RequestVerificationToken"]').val()
        },
        success: function (result) {
            var $div = $(result);
            var divHTML = $div.find(returnDiv).html();
            $(returnDiv).html(divHTML);
        }
    });
}

document.getElementById('userSelect').onchange = function showUsersRoles(event) {
    event.preventDefault;
    var userid = $("#userSelect").val();
    var handler = "/Dashboard?handler=FindUser";
    var div = "#roleEditingDiv";
    var jsonData = { user: userid};
    ajaxPost(jsonData, handler, div);
}


$(document).on('click', '#addUserRole', function addUserRole(event) {
    event.preventDefault;
    var user = $("#userBeingAltered").val();
    var role = $("#userNewRole").val();
    var jsonData = { userBeingAltered: user, newUserRole: role };
    var handler = "/Dashboard?handler=UserRoleAdd";
    var div = "#roleEditingDiv";
    ajaxPost(jsonData, handler, div);
});

$(document).on('click', '#deleteUserRole', function (event) {
    event.preventDefault;
    var user = $("#userBeingAltered").val();
    var role = $("#deleteUserRole").val();
    var jsonData = { userBeingAltered: user, newUserRole: role };
    var handler = "/Dashboard?handler=UserRoleRemove";
    var div = "#roleEditingDiv";
    ajaxPost(jsonData, handler, div);
});


