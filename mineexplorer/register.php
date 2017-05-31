<?php
require 'Scores.php';

if ($_SERVER['REQUEST_METHOD'] == 'POST') {
	$request = json_decode(file_get_contents("php://input"), true);
    $id = Scores::registerPlayer($request['Nick'], $request['RegisterDate']);

    if ($id) {
        $response["Status"] = 1;
        $response["Id"] = $id[0]['id'];

        print json_encode($response);
    }
    else {
        print json_encode(array("Status" => 2, "Message" => "Could not register the nick, nick already exists or empty."));
    }
}
?>