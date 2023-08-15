var canvas = document.getElementById('canvas');
var ctx = canvas.getContext('2d');
var canvasx = $(canvas).offset().left;
var canvasy = $(canvas).offset().top;
var last_mousex = last_mousey = 0;
var mousex = mousey = 0;
var mousedown = false;
var tooltype = 'draw';



document.addEventListener('DOMContentLoaded', function () {

    var messageInput = document.getElementById('message');
    var wordInput = document.getElementById('wordinput');

    // Get the user name and store it to prepend to messages.
    var name;
    while(!name || name.length < 3)
        name = prompt('Enter your name:', '');
    // Set initial focus to message input box.
    messageInput.focus();

    // Start the connection.
    var connection = new signalR.HubConnectionBuilder()
                        .withUrl('/chat')
                        .build();

    // Create a function that the hub can call to broadcast messages.
    connection.on('broadcastMessage', function (name, message) {
        // Html encode display name and message.
        var encodedName = name;
        var encodedMsg = message;
        // Add the message to the page.
        var liElement = document.createElement('li');
        liElement.innerHTML = '<strong>' + encodedName + '</strong>:&nbsp;&nbsp;' + encodedMsg;
        var discuss = document.getElementById('discussion').appendChild(liElement);
        if(document.querySelectorAll("li").length > 23)
        {
            document.querySelector("li").remove();
        }
        
    });

    connection.on('clearCanvas', function(){
        console.warn('clear canvas')
        ctx.clearRect(0, 0, canvas.width, canvas.height);
    });
    


    // Transport fallback functionality is now built into start.
    connection.start()
        .then(function () {
            document.getElementById('sendmessage').addEventListener('click', function (event) {
                // Call the Send method on the hub.
                if(!messageInput.value || messageInput.value.length < 2)
                {
                    messageInput.value = "";
                    messageInput.focus();
                    return;
                }
                connection.invoke('send', name, messageInput.value);

                // Clear text box and reset focus for next comment.
                messageInput.value = '';
                messageInput.focus();
                event.preventDefault();
            });
    })
    .then (function (){
        connection.invoke('addplayer', name);
    })
    .then (function(){
        document.getElementById('ready').addEventListener('click', function (event) {
            // Call the Send method on the hub.
            connection.invoke('send', name, "/ready");
            event.preventDefault();
            document.getElementById('start-game').style.display = "none";
        });
    })
    .then(function () {
        document.getElementById('sendword').addEventListener('click', function (event) {
            // Call the Send method on the hub.
            if(!wordInput.value || wordInput.value.length < 2)
            {
                wordInput.value = "";
                wordInput.focus();
                return;
            }
            connection.invoke('AddNewWord', wordInput.value);

            // Clear text box and reset focus for next comment.
            wordInput.value = '';
            wordInput.focus();
            event.preventDefault();
        });
    })
    .catch(error => {
        console.error(error.message);
    });
});





$(canvas).on('mousedown', function (e) {
    last_mousex = mousex = parseInt(e.clientX - canvasx);
    last_mousey = mousey = parseInt(e.clientY - canvasy);
    mousedown = true;
});

$(canvas).on('mouseup', function (e) {
    mousedown = false;
});

var drawCanvas = function (prev_x, prev_y, x, y, clr) {
    ctx.beginPath();
    ctx.globalCompositeOperation = 'source-over';
    ctx.strokeStyle = clr
    ctx.lineWidth = 3;
    ctx.moveTo(prev_x, prev_y);
    ctx.lineTo(x, y);
    ctx.lineJoin = ctx.lineCap = 'round';
    ctx.stroke();
};

$(canvas).on('mousemove', function (e) {
    mousex = parseInt(e.clientX - canvasx);
    mousey = parseInt(e.clientY - canvasy);
    var clr = $('select[id=color]').val()

    if ((last_mousex > 0 && last_mousey > 0) && mousedown) {
        drawCanvas(mousex, mousey, last_mousex, last_mousey, clr);
        connection.invoke('draw', last_mousex, last_mousey, mousex, mousey, clr);
    }
    last_mousex = mousex;
    last_mousey = mousey;

    $('#output').html('current: ' + mousex + ', ' + mousey + '<br/>last: ' + last_mousex + ', ' + last_mousey + '<br/>mousedown: ' + mousedown);
});

var mouse_down = false;
var connection = new signalR.HubConnectionBuilder()
    .withUrl('/draw')
    .build();

connection.on('draw', function (prev_x, prev_y, x, y, clr) {
    drawCanvas(prev_x, prev_y, x, y, clr);
});

connection.start();

clearMousePositions = function () {
    last_mousex = 0;
    last_mousey = 0;
}