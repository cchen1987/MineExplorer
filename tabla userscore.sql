CREATE TABLE IF NOT EXISTS UserScore (
  id int(11) NOT NULL AUTO_INCREMENT,
  nick varchar(20) CHARACTER SET utf8 NOT NULL,
  registerDate datetime NOT NULL,
  beginner_score float DEFAULT NULL,
  intermediate_score float DEFAULT NULL,
  expert_score float DEFAULT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY nick (nick)
)