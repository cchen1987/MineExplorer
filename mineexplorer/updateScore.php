<?php
require 'Scores.php';

if ($_SERVER['REQUEST_METHOD'] == 'POST') {
    $request = json_decode(file_get_contents("php://input"), true);
    if (isset($request['Nick'])) {
        $result = Scores::update($request['Id'], $request['Nick'], $request['RegisterDate'], $request['Type'], $request['Score']);
    } else {
        $result = Scores::update($request['Id'], null, $request['RegisterDate'], $request['Type'], $request['Score']);
    }

    if ($result) {
        print json_encode(array("Status" => 1,"Message" => "Your current rank is ".$result[0]['rank']." of ".$result[0]['total']." players!"));;
    } else {
        print json_encode(array("Status" => 2,"Message" => "Error updating your score."));
    }
}
?>