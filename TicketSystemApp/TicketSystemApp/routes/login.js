'use strict';
var express = require('express');
var router = express.Router();
var request = require('request');

router.get('/', function (req, res) {
    res.render('login', { title: 'Ticket System' });
});

router.post('/', function (req, res, next) {

    request.post('http://localhost:5000/api/Customers/Login', {
        json: {
            loginId: req.body.loginId,
            password: req.body.password
        }
    }, (error, response, body) => {
        if (!error && response.statusCode == 200) {
            if (body) {
                req.app.locals.typeOfAuthenticated = 1;
                res.redirect('/users');
            } else {
                res.render('index', { title: "Ticket System" });
            }
        }
    });
});

module.exports = router;