<?php
require 'Scores.php';

if ($_SERVER['REQUEST_METHOD'] == 'POST') {
	$request = json_decode(file_get_contents("php://input"), true);
    $scores = Scores::getScoresAtIndex($request['Index'], $request['Type']);

    if ($scores) {
        $response["Status"] = 1;
        $response["Scores"] = $scores;

        print json_encode($response);
    }
    else {
        print json_encode(array("Status" => 2, "Message" => "There are no players."));
    }
}
?>