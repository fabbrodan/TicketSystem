'use strict';
var express = require('express');
var router = express.Router();
var request = require('request');

router.get('/login', function (req, res) {
    res.render('admin/login');
});

router.post('/login', function (req, res) {

    request.post("http://127.0.0.10/api/Admin/Login", {
        json: {
            loginId: req.body.loginId,
            password: req.body.password
        }
    },
        (error, response, body) => {
            if (!error && response.statusCode == 200) {
                if (body != null) {
                    req.app.locals.typeOfAuthenticated = 2;  
                    req.app.locals.adminId = body.adminId;
                }
                res.render('index');
            }
        });
});

module.exports = router;