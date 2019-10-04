/*
    App.js,
    03.10.2019
 */

/*
    webSocketHandle
    handling the connection to the WSS
 */
webSocketHandle = {
    /*
        [f] start,
        starts the session to the WSS,
     */
    start: function () {
        this.webSocket = new WebSocket("ws://192.168.1.15:7777/");
        this.webSocket.onopen = function () {
            gameHandle.openMenu();
        };
        this.webSocket.onmessage = function (evt) {
            webSocketHandle.onMessage(evt);
        };
        this.webSocket.onclose = function () {
            webSocketHandle.onClose();
        };
    },
    /*
        [f] onOpen,
        sends client information to the WSS;
        - messageType: 0,
        - name,
        - color
    */
    onOpen: function (name, color) {
        this.sendMessage("type=" + "0" + "&" + "name=" + name + "&" + "color=" + color);
        gameHandle.startGame();
    },
    /*
        [f] WebSocketEvent
        nSocketClose: reload the page for reconnect
    */
    onClose: function () {


        window.location.replace("index.html");
    },
    /*
        [f] WebSocketEvent
        sendMessage to WebSocketServer
    */
    sendMessage: function (message) {
        /*
        [f] WebSocketEvent
        sendMessage to WebSocketServer
         */
        this.webSocket.send(message);
    },

    /*
    [f] WebSocketEvent
    called by receiving messages from WebSocketServer
     */
    onMessage: function (evt) {
        let args = [];
        let pairs = evt.data.toString().split("&");
        for (let i = 0; i < pairs.length; i++) {
            let keyValue = pairs[i].split("=");
            let key = keyValue[0];
            args[key.toUpperCase()] = keyValue[1];
        }
        if (args["TYPE"] === "3") {
            let x = parseFloat(args["X"]);
            let y = parseFloat(args["Y"]);
            let size = parseFloat(args["SIZE"]);
            let gameObject = new Object(args["ID"], args["NAME"], args["COLOR"], size, size, x, y);
            gameHandle.gameObjects.push(gameObject);
        } else if (args["TYPE"] === "2") {
            for (let i = 0; i < gameHandle.gameObjects.length; i++) {
                let gameObject = gameHandle.gameObjects[i];
                if (gameObject.id === args["ID"]) {
                    gameObject.isLocalPlayer = true;
                }
            }
        } else if (args["TYPE"] === "5") {
            let index = -1;
            for (let i = 0; i < gameHandle.gameObjects.length; i++) {
                let gameObject = gameHandle.gameObjects[i];
                if (gameObject.id === args["ID"]) {
                    index = i;
                }
            }
            if (index > -1) {
                gameHandle.gameObjects.splice(index, 1);
            }
        } else if (args["TYPE"] === "4") {
            for (let i = 0; i < gameHandle.gameObjects.length; i++) {
                let gameObject = gameHandle.gameObjects[i];
                if (gameObject.id === args["ID"]) {
                    gameObject.width = args["SIZE"];
                    gameObject.height = args["SIZE"];
                    gameObject.color = args["COLOR"];
                    if (!gameObject.isLocalPlayer) {
                        gameObject.x = parseFloat(args["X"]);
                        gameObject.y = parseFloat(args["Y"]);
                    }
                }
            }
        }
    }
};

/*
    gameHandle

    handling game on client site
 */
gameHandle = {
    /*
        [f] startGame

     */
    startGame: function () {
        this.gameObjects = [];
        myGameArea.start();
        $("#overlay").show();
        this.playerDeath();
    },
    /*
    [f] openMenu
    actives the settings menu
    */
    openMenu: function () {
        $("#loadingScreen").hide();
        $("#menu").show();
        let form = document.querySelector('form');
        form.onsubmit = function () {
            let name = document.querySelector('input[name=name]').value;
            let color = document.querySelector('input[name=hat-color]:checked').value;
            webSocketHandle.onOpen(name, color);
            $("#menu").hide();
            return false;
        };
    },
    playerDeath: function () {
    }
};
/*
    myGameArea

 */
const myGameArea = {
    canvas: document.createElement("canvas"),

    start: function () {
        this.size = 0;
        if (document.body.clientWidth >= document.body.clientHeight) {
            this.size = document.body.clientHeight;
        } else {
            this.size = document.body.clientWidth;
        }
        this.canvas.width = this.size;
        this.canvas.height = this.size;

        this.context = this.canvas.getContext("2d");
        document.body.insertBefore(this.canvas, document.body.childNodes[0]);
        this.interval = setInterval(updateGameArea, 10);
        window.addEventListener('keydown', function (e) {
            myGameArea.keys = (myGameArea.keys || []);
            myGameArea.keys[e.keyCode] = (e.type == "keydown");
        });
        window.addEventListener('keyup', function (e) {
            myGameArea.keys[e.keyCode] = (e.type == "keydown");
        });

    },
    clear: function () {
        this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);
    }
};

/*
    object
    create or update a object

 */
function Object(id, name, color, width, height, x, y) {
    this.id = id;
    this.name = name;
    this.gamearea = myGameArea;
    this.width = width;
    this.height = height;
    this.x = x;
    this.y = y;
    this.speedX = 0;
    this.speedY = 0;
    this.color = color;
    this.isLocalPlayer = false;
    this.score = 20;
    this.update = function () {
        this.x = this.x + this.speedX / 6;
        this.y = this.y + this.speedY / 6;
        if (this.x >= 100 - width) {
            this.x = 100 - width;
        } else if (this.x <= 0) {
            this.x = 0;
        }
        if (this.y >= 100 - height) {
            this.y = 100 - height;
        } else if (this.y <= 0) {
            this.y = 0;
        }
        let ctx = this.gamearea.context;
        ctx.fillStyle = this.color;
        if (this.isLocalPlayer === true) {
            webSocketHandle.sendMessage("type=" + "1" + "&" + "x=" + this.x + "&" + "y=" + this.y);
        }

        ctx.beginPath();
        ctx.arc(this.x / 100 * ctx.canvas.width, this.y / 100 * ctx.canvas.height, this.height / 100 * ctx.canvas.height, 0, 2 * Math.PI);
        ctx.fill();
        ctx.fillStyle = '#fff';
        ctx.textAlign = "center";
        ctx.font = "16px Inconsolata";
        ctx.fillText(this.name, this.x / 100 * ctx.canvas.width, this.y / 100 * ctx.canvas.height);

        // update score overlay
        $("#overlay-counter-score").text(this.score);
    };
}

function updateGameArea() {
    myGameArea.clear();
    for (let i = 0; i < gameHandle.gameObjects.length; i++) {
        let gameObject = gameHandle.gameObjects[i];
        gameObject.speedX = 0;
        gameObject.speedY = 0;
        if (gameObject.isLocalPlayer) {
            if (myGameArea.keys && myGameArea.keys[37]) {
                gameObject.speedX = -1;
            }
            if (myGameArea.keys && myGameArea.keys[39]) {
                gameObject.speedX = 1;
            }
            if (myGameArea.keys && myGameArea.keys[38]) {
                gameObject.speedY = -1;
            }
            if (myGameArea.keys && myGameArea.keys[40]) {
                gameObject.speedY = 1;
            }
        }
        gameObject.update();
    }
}

$(document).ready(function () {
    webSocketHandle.start();
    $("#menu").hide();
    $("#overlay").hide();
});