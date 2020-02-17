'use strict';
var express = require('express');
var router = express.Router();

/* GET home page. */
router.get('/', function (req, res) {   
    console.log(req.app.locals.typeOfAuthenticated);
    console.log(req.app.locals.customerId);
    res.render('index', { title: 'Ticket System' });
});

router.get('/login', function (req, res) {
    res.render('login');
});

router.get('/signout', function (req, res) {
    req.app.locals.typeOfAuthenticated = 0;
    console.log(req.app.locals.typeOfAuthentication);
    res.redirect('/');
});

module.exports = router;
