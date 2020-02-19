'use strict';
var express = require('express');
var router = express.Router();
var request = require('request');

router.get('/', function (req, res) {
    res.render('login', { title: 'Ticket System' });
});

router.post('/', function (req, res) {

    request.post('http://127.0.0.10/api/Customers/Login', {
        json: {
            loginName: req.body.loginId,
            password: req.body.password
        }
    }, (error, response, body) => {
            if (!error && response.statusCode == 200) {
                if (body !== null) {
                    req.app.locals.typeOfAuthenticated = 1;
                    req.app.locals.customerId = body.customerId;
                    res.redirect('users/'+body.customerId);
                } else {
                    res.render('index', { title: "Ticket System" });
                }
            } else if (response.statusCode == 204) {
                res.render('login', { success: false });
            }

    });
});

router.post('/newUser', function (req, res) {

    request.post('http://127.0.0.10/api/Customers/NewUser', {
        json: {
            LoginName: req.body.loginId,
            Email: req.body.email,
            PhoneNumber: req.body.phone,
            Password: req.body.password
        }
    },
        (error, response, body) => {
            if (!error && response.statusCode == 200) {
                if (body != null) {
                    req.app.locals.typeOfAuthenticated = 1;
                    req.app.locals.customerId = body.customerId;
                    res.redirect('users/' + body.customerId);
                }
            }
            else {
                res.redirect('back');
            }
        });
});

module.exports = router;