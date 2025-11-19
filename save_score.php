<?php
$servername = "localhost";
$username = "root";     // default XAMPP username
$password = "";         // default XAMPP password
$dbname = "database_sal";

$conn = new mysqli($servername, $username, $password, $dbname);

if ($conn->connect_error) {
    die("Connection failed: " . $conn->connect_error);
}

$playerName = $_POST['playerName'];
$playTime = $_POST['playTime'];

$sql = "INSERT INTO leaderboard (player_name, play_time)
        VALUES ('$playerName', '$playTime')";

if ($conn->query($sql) === TRUE) {
    echo "SUCCESS";
} else {
    echo "ERROR: " . $conn->error;
}

$conn->close();
?>
