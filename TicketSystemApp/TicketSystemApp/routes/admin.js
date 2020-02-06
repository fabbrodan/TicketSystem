'use strict';
var express = require('express');
var router = express.Router();
var request = require('request');

router.get('/', function (req, res) {
    res.render('login');
});

router.post('/login', function (req, res) {

    request.post("http://localhost:5000/api/Admin/Login", {
        json: {
            loginId: req.body.loginId,
            password: req.body.password
        }
    },
        (error, response, body) => {
            if (!error && response.statusCode == 200) {
                if (body) {
                    req.app.locals.typeOfAuthenticated = 2;
                    res.redirect('/');
                }
            }
        });
});

module.exports = router;