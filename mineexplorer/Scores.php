<?php
require 'Database.php';

class Scores
{
    function __construct()
    {
    }

    public static function registerPlayer($nick, $registerDate) {
        $sql = "INSERT INTO UserScore VALUES (default, ?, ?, null, null, null)";

        try {
            $command = Database::getInstance()->getDb()->prepare($sql);
            $command->execute(array($nick, $registerDate));

            $sql = "SELECT id FROM UserScore WHERE nick = ? AND registerDate = ?";
            
            $command = Database::getInstance()->getDb()->prepare($sql);
            $command->execute(array($nick, $registerDate));

            return $command->fetchAll(PDO::FETCH_ASSOC);
        } catch (PDOException $e) {
            return false;
        }
    }

    public static function getScoreById($id) {
        $sql = "SELECT id as Id, nick as Nick, registerDate as RegisterDate, beginner_score as BeginnerScore, intermediate_score as IntermediateScore, expert_score as ExpertScore FROM UserScore WHERE id = $id";
        try {
            $command = Database::getInstance()->getDb()->prepare($sql);
            $command->execute();

            return $command->fetchAll(PDO::FETCH_ASSOC);
        } catch (PDOException $e) {
            return false;
        }
    }

    public static function getScoresAtIndex($index, $columnName) {
        $sql = "SELECT id as Id, nick as Nick, registerDate as RegisterDate, beginner_score as BeginnerScore, intermediate_score as IntermediateScore, expert_score as ExpertScore FROM UserScore WHERE $columnName IS NOT NULL ORDER BY $columnName ASC LIMIT $index, 10";

        try {
            $command = Database::getInstance()->getDb()->prepare($sql);
            $command->execute();

            return $command->fetchAll(PDO::FETCH_ASSOC);
        } catch (PDOException $e) {
            return false;
        }
    }

    public static function update($id, $nick, $registerDate, $columnName, $score) {
        $sql = "UPDATE UserScore SET $columnName = $score";
        $sql2 = " WHERE id = $id AND registerDate = '$registerDate'";
        if (!is_null($nick) && trim($nick) != "") {
            $sql .= ", nick = '$nick' ";
        }
        $sql .= $sql2;

        try {
            $command = Database::getInstance()->getDb()->prepare($sql);
            $rowsAfected = $command->execute();

            if ($rowsAfected) {
                $sql = "SELECT rank, total
                          FROM
                             (SELECT @rank:=@rank+1 AS rank, id, registerDate
                                FROM UserScore, (SELECT @rank := 0) r
                               WHERE $columnName IS NOT NULL AND $columnName != 0
                               ORDER BY $columnName ASC
                             ) t, (SELECT COUNT(*) AS total FROM UserScore) f
                         WHERE t.id = $id AND t.registerDate = '$registerDate'";

                $command = Database::getInstance()->getDb()->prepare($sql);
                $command->execute();
                return $command->fetchAll(PDO::FETCH_ASSOC);
            } else {
                return false;
            }
        } catch (PDOException $e) {
            return false;
        }
    }
}
?>
