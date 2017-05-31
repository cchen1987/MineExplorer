<?php
require 'Scores.php';

if ($_SERVER['REQUEST_METHOD'] == 'GET') {
    $id = $_GET['id'];
    $scores = Scores::getScoreById($id);

    if ($scores) {
        $response["Status"] = 1;
        $response["Scores"] = $scores;

        print json_encode($response);
    }
    else {
        print json_encode(array("Status" => 2, "Message" => "There are no players with this id: $id."));
    }
}
?>