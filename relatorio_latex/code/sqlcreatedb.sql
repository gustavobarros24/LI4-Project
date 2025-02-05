CREATE DATABASE AutoBrick;
GO
USE AutoBrick;
GO

CREATE TABLE piece (
    id_piece INT PRIMARY KEY,
    stock INT NOT NULL,
    nome VARCHAR(45) NOT NULL
);

CREATE TABLE [set] (
    id_set INT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL
);

CREATE TABLE set_has_piece (
    set_id_set INT NOT NULL,
    piece_id_piece INT NOT NULL,
    quantity INT NOT NULL,
    PRIMARY KEY (set_id_set, piece_id_piece),
    FOREIGN KEY (set_id_set) REFERENCES [set](id_set),
    FOREIGN KEY (piece_id_piece) REFERENCES piece(id_piece)
);

CREATE TABLE [user] (
    id_user INT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    senha VARCHAR(40) NOT NULL
);

CREATE TABLE encomenda (
    id_encomenda INT PRIMARY KEY,
    isfulfillable TINYINT NOT NULL,
    isfinished TINYINT NOT NULL,
    user_id_user INT,
    FOREIGN KEY (user_id_user) REFERENCES [user](id_user)
);

CREATE TABLE encomenda_has_set (
    encomenda_id_encomenda INT NOT NULL,
    set_id_set INT NOT NULL,
    PRIMARY KEY (encomenda_id_encomenda, set_id_set),
    FOREIGN KEY (encomenda_id_encomenda) REFERENCES encomenda(id_encomenda),
    FOREIGN KEY (set_id_set) REFERENCES [set](id_set)
);

CREATE TABLE ManualPage (
    id_manualpagepath VARCHAR(100) PRIMARY KEY,
    set_id_set INT NOT NULL,
    FOREIGN KEY (set_id_set) REFERENCES [set](id_set)
);

CREATE TABLE admin (
    code INT PRIMARY KEY
);