// Lets get the current game id
var MAZE_ID = window.localStorage.getItem('MAZE_ID')
getCurrentMaze()
// Directions
var SOUTH = 1
var EAST = 2
var NORTH = 4
var WEST = 8
// AutoPlay
var AUTO_PLAY = false;
var KEYBOARD = false;

if (KEYBOARD) {

    document.onkeydown = function(e) {

        e = e || window.event;

        if (e.key == "ArrowUp") {
            makeMove(NORTH)
        }
        else if (e.key == "ArrowDown") {
            makeMove(SOUTH)
        }
        else if (e.key == "ArrowLeft") {
            makeMove(WEST)
        }
        else if (e.key == "ArrowRight") {
            makeMove(EAST)
        }

    }
}

// Button listeners
document.getElementById("start").addEventListener('click', function(){
    startGame()
})
document.getElementById("left").addEventListener('click', function(){
    makeMove(WEST)
})
document.getElementById("right").addEventListener('click', function(){
    makeMove(EAST)
})
document.getElementById("up").addEventListener('click', function(){
    makeMove(NORTH)
})
document.getElementById("down").addEventListener('click', function(){
    makeMove(SOUTH)
})
document.getElementById("auto").addEventListener('click', function(){
    autoPlay()
})

function startGame() {
    document.getElementById("auto").innerHTML = "▶"
    createMaze()
}

function autoPlay () {
    AUTO_PLAY = !AUTO_PLAY
    if(AUTO_PLAY) {
        getCurrentMaze()
        document.getElementById("auto").innerHTML = "❚❚"
    } else {
        document.getElementById("auto").innerHTML = "▶"
    }
}

// Create a maze, Make POST API call to backend
async function createMaze() {
    axios.post('https://ponychallenge.trustpilot.com/pony-challenge/maze', 
    {
        
            "maze-width": parseInt(document.getElementById("mw").value),
            "maze-height": parseInt(document.getElementById("mh").value),
            "maze-player-name": document.getElementById("pn").value,
            "difficulty": parseInt(document.getElementById("d").value)
    })
    .then((response) => {
        MAZE_ID = response.data.maze_id
        // Lets always store as current
        window.localStorage.setItem('MAZE_ID', MAZE_ID)
        // And Lets print it
        AUTO_PLAY = false;
        updateMaze()
        },
        (error) => {
            console.log(error)
            document.getElementById("mazeprint").innerHTML = error
        }
    )
}

// Lets get the current state of the maze and analyze, Make GET API call to backend
async function getCurrentMaze() {
    axios.get('https://ponychallenge.trustpilot.com/pony-challenge/maze/' + MAZE_ID)
    .then((response) => {
        AnalyzeMaze(response.data)
        },
        (error) => {
            console.log(error)
        }
    )
}

// Get updated layout to print, Make GET API call to backend
async function updateMaze(){
    axios.get('https://ponychallenge.trustpilot.com/pony-challenge/maze/' + MAZE_ID + '/print')
    .then((response) => {
        document.getElementById("mazeprint").innerHTML = response.data
        if(AUTO_PLAY)
            getCurrentMaze()
        },
        (error) => {
            console.log(error)
            document.getElementById("mazeprint").innerHTML = error
        }
    )
}

// Lets make a move, Make POST API call to backend
async function makeMove(direction) {

    var directionJSON = {}
    if (direction == NORTH) {
        directionJSON = {
            "direction" : "north"
        }
    } else if (direction == SOUTH) {
        directionJSON = {
            "direction" : "south"
        }
    } else if (direction == EAST) {
        directionJSON = {
            "direction" : "east"
        }
    } else if (direction == WEST) {
        directionJSON = {
            "direction" : "west"
        }
    } else {
        console.log("Invalid direction")
        return
    }
    
    axios.post('https://ponychallenge.trustpilot.com/pony-challenge/maze/' + MAZE_ID, directionJSON)
    .then((response) => {
        updateMaze()
        },
        (error) => {
            console.log(error)
        }
    )
}

// Lets analyze the maze and make a move
function AnalyzeMaze(maze_data) {

    var maze_id = maze_data['maze_id']
    var maze = maze_data['data']
    var domokun = maze_data['domokun']
    var endpoint = maze_data['end-point']
    var pony = maze_data['pony']
    var size = maze_data['size']
    var game_state = maze_data['game-state']
    var maze_width = size[0]
    var maze_difficulty = maze_data['difficulty']

    // If game is over, lets get it over with
    if (game_state.state == "over" || game_state.state == "won") {
        document.getElementById("mazeprint").innerHTML = ""
        var image = document.createElement("img")
        var imageParent = document.getElementById("mazeprint")
        image.id = "game_img"
        image.width = 600
        image.height = 500
        image.className = "game_image"
        image.src = 'https://ponychallenge.trustpilot.com' + game_state['hidden-url']
        imageParent.appendChild(image)
        return
    }

    var mazeCompiled = []
    compileMaze(maze, maze_width, mazeCompiled)
    
    var explore_path = []
    var maze_all_paths = []

    // eplore all paths of pony
    var loop = exploreAll(mazeCompiled, maze_width, pony[0], -1, explore_path, maze_all_paths)

    var longest_path_to_run = -1
    var short_path_to_end = -1
    var short_path_to_domokun = -1
    var domokun_range = -1

    // All paths that lead to endpoint
    maze_all_paths.forEach((path, index, thisarray) => {

        // see which all paths have end 
        var end_found = path.findIndex(function(position) {
            return position == endpoint[0]
        })
        
        // see where domokun is
        var domokun_found = path.findIndex(function(position) {
            return position == domokun[0]
        })

        // capture the shortest path
        if((end_found != -1) && (short_path_to_end == -1 || thisarray[short_path_to_end].length >= path.length)) {
            short_path_to_end = index

            // See if domokun is also found on same path
            if(domokun_found != -1)
                domokun_range = domokun_found
        }

        // if found, look for shortest path to see if its in range
        if(domokun_found != -1) {
            
            if(short_path_to_domokun == -1 || thisarray[short_path_to_domokun].length >= path.length)
                short_path_to_domokun = index
    
        // to run away from domokun
        } else {
            if(longest_path_to_run == -1 || thisarray[longest_path_to_run].length <= path.length)
                longest_path_to_run = index
        }  

    })

    var solution_path_moves = []
    var runaway_path_moves = []

    solution_path_moves = maze_all_paths[short_path_to_end].map(mapMoves)
    
    // that means, all paths of pony has domokun in it. :(
    if (longest_path_to_run != -1)
        runaway_path_moves = maze_all_paths[longest_path_to_run].map(mapMoves)
    else
        runaway_path_moves = maze_all_paths[short_path_to_end].map(mapMoves)

    // domokun is in our way to end
    if (domokun_range != -1) {
        // domokun is in range, definitely move away (danger zone)
        if(domokun_range <= maze_difficulty + 2)
            makeMove(runaway_path_moves[0])
        else
            makeMove(solution_path_moves[0])
    // domokun is not in our way to end
    } else {
        // domokun is in range, might be hiding somewhere
        if (maze_all_paths[short_path_to_domokun].length - 1 <= maze_difficulty + 2) {

            var common_path = -1
            maze_all_paths[short_path_to_domokun].forEach((position, index) => {
                if(index < maze_all_paths[short_path_to_end].length && position == maze_all_paths[short_path_to_end][index]){
                    common_path = index
                }
            })
            // if hiding but we can still run through common path
            if(common_path < (maze_all_paths[short_path_to_domokun].length - 1)/2)
                makeMove(solution_path_moves[0])
            // if hiding but we can't cross then run away
            else
                makeMove(runaway_path_moves[0])
        // not in range
        } else
            makeMove(solution_path_moves[0])

    }
}

// explore the maze by exploring all possible paths
function exploreAll(compiledMaze, mazeWidth, currentPos, lastPos, explorepath, all_paths) {

    // Lets see if have already explored this positions
    var found = explorepath.findIndex(function(position) {
        return position == currentPos
    })

    // Are we in loop?? Is it possible??
    if(found != -1)
        return true

    // Let the explorations begin
    explorepath.push(currentPos)
        
    // explore north
    if ((compiledMaze[currentPos] & NORTH) && lastPos != (currentPos-mazeWidth))
        exploreAll(compiledMaze, mazeWidth, currentPos-mazeWidth, currentPos, explorepath, all_paths)

    // explore south
    if ((compiledMaze[currentPos] & SOUTH) && lastPos != (currentPos+mazeWidth))
        exploreAll(compiledMaze, mazeWidth, currentPos+mazeWidth, currentPos, explorepath, all_paths)
    
    // explore east
    if ((compiledMaze[currentPos] & EAST) && lastPos != (currentPos+1))
        exploreAll(compiledMaze, mazeWidth, currentPos+1, currentPos, explorepath, all_paths)

    // explore west
    if ((compiledMaze[currentPos] & WEST) && lastPos != (currentPos-1))
        exploreAll(compiledMaze, mazeWidth, currentPos-1, currentPos, explorepath, all_paths)

    // lets capture all the paths
    all_paths.push(explorepath.map(function(value) {
        return value
    }))
    // we hit the deadend
    explorepath.pop(currentPos)
    return false
    
}

function mapMoves(value, index, thisarray){
    var nextvalue = -1
    if(index+1 <= thisarray.length-1)
        nextvalue = thisarray[index+1]
        
    if((nextvalue-value) == 1)
        return EAST
    else if ((nextvalue-value) == -1)
        return WEST
    else if ((nextvalue-value) > 1)
        return SOUTH
    else if ((nextvalue-value) < -1)
        return NORTH
}

// wnes   - west north east south
// 0000 0 - Not possible                            1000 8 - can travel west
// 0001 1 - Can travel South                        1001 9 - can travel west or south
// 0010 2 - Can travel East                         1010 10 - can travel west or east
// 0011 3 - Can travel south or east                1011 11 - can travel west or south or east
// 0100 4 - Can travel north                        1100 12 - can travel west or north
// 0101 5 - can travel north or south               1101 13 - can travel west or north or south
// 0110 6 - can travel north or east                1110 14 - can travel west or north or east
// 0111 7 - can travel north or south or east       1111 15 - Not possible
function compileMaze(maze, mazeWidth, mazeCompiled) {
    // lets walk through all the cells and capture bits for travel restrictions
    // 0 - means we cannot travel
    // 1 - means we can travel
    maze.forEach((cellElem, index, thismaze) => {

        // See if we can travel north and west
        var NSEW = getNSEW(cellElem)
        
        // See if we can travel east
        if (index+1 <= thismaze.length-1) {
            var rightNSEW = getNSEW(thismaze[index+1])
            rightNSEW = rightNSEW & WEST
    
            if(rightNSEW == 0)
                NSEW = NSEW & ~EAST
            else 
                NSEW = NSEW | EAST
         } else
            NSEW = NSEW & ~EAST
    

        // See if we can travel south
        if (index + mazeWidth <= thismaze.length-1) {
            var bottomNSEW = getNSEW(thismaze[index+mazeWidth])
            bottomNSEW = bottomNSEW & NORTH

            if (bottomNSEW == 0)
                NSEW = NSEW & ~SOUTH
            else
                NSEW = NSEW | SOUTH
        }
        else 
            NSEW = NSEW & ~SOUTH

        
        mazeCompiled.push(NSEW)
        
    })
}

function getNSEW (cellElem) {

    // west, north wall. cannot travel west and north
    if(cellElem.length == 2) {

        return SOUTH | EAST

    // No wall, we can definitely travel north and west.
    } else if (cellElem.length == 0) {

        return NORTH | WEST

    // west wall, cannot travel west for sure.
    }else if(cellElem.length == 1 && cellElem[0][0] == 'w') {

        return NORTH | SOUTH | EAST
    
    // north wall, cannot travel north for sure.
    }else {

        return WEST | SOUTH | EAST
    }
}
