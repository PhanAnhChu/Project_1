drop database if exists Minimart;
create database Minimart;

use Minimart;

create table cashiers (
	id int primary key auto_increment,
    name varchar(32) NOT NULL,
    username varchar(64) UNIQUE,
    password varchar(64) NOT NULL,
    status tinyint(1) NOT NULL
);

Create table goods (
	id int primary key AUTO_INCREMENT,
    name varchar(32) NOT NULL,
    price float NOT NULL,
    quantity int NOT NULL
);

create table customers (
	id int primary key AUTO_INCREMENT,
    name varchar(32) NOT NULL,
    phone varchar(10),
    reward_point int NOT NULL
);

CREATE TABLE bills (
  id int primary key AUTO_INCREMENT,
  cashier_id int NOT NULL,
  created_date datetime NOT NULL,
  customer_id int NOT NULL,
  FOREIGN KEY (cashier_id) REFERENCES cashiers(id),
  FOREIGN KEY (customer_id) REFERENCES customers(id)
);

CREATE Table orders (
	id int primary key auto_increment,
    bill_id int not null,
    good_id int NOT NULL,
    quantity int NOT NULL,
    price float NOT NULL,
    
    foreign key (bill_id) references bills(id),
    foreign key (good_id) references goods(id)
);

create table shifts (
    id int primary key AUTO_INCREMENT,
    start_time datetime NOT NULL,
    end_time datetime NOT NULL,
    reporter_id int NOT NULL,
    expected_income float NOT NULL,
    actual_income float NOT NULL,
    confirmer_id int NOT NULL,
  
	foreign key (reporter_id) references cashiers(id),
    foreign key (confirmer_id) references cashiers(id)
);

insert into customers (name, phone, reward_point)
values ('Khach Vang Lai', null, 0);

insert into cashiers (name, username, password, status)
values ('Phan Anh', 'anhcp7978', '1234', true), ('Minh Duc', 'mesterdbd', '1234', true);

INSERT INTO goods (id, name, price, quantity)
VALUES
  (1, 'eggs', 3000, 100),
  (2, 'milk', 7000, 100),
  (3, 'bread', 3000, 100),
  (4, 'cheese', 10000, 100),
  (5, 'yogurt', 12000, 100),
  (6, 'chicken', 150000, 100),
  (7, 'beef', 50000, 100),
  (8, 'pork', 40000, 100),
  (9, 'fish', 30000, 100),
  (10, 'shrimp', 6000, 100),
  (11, 'lettuce', 10000, 100),
  (12, 'tomatoes', 10000, 100),
  (13, 'potatoes', 10000, 100),
  (14, 'carrots', 10000, 100),
  (15, 'onions', 10000, 100),
  (16,'garlic', 10000, 100),
  (17,'ginger', 10000, 100),
  (18,'apples', 10000, 100),
  (19,'oranges', 70000, 100),
  (20,'bananas', 80000, 100);

CREATE USER if not exists 'pf1121'@'localhost' IDENTIFIED BY 'pf1121';
GRANT ALL PRIVILEGES ON minimart.* TO 'pf1121'@'localhost' WITH GRANT OPTION;