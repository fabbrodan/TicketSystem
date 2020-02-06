'use strict';
var express = require('express');
var router = express.Router();

/* GET home page. */
router.get('/', function (req, res) {   
    res.render('index', { title: 'Ticket System' });
});

router.get('/login', function (req, res) {
    res.render('login');
});

module.exports = router;
