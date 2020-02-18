'use strict';
var express = require('express');
var router = express.Router();
var request = require('request');
var XMLHttpRequest = require("xmlhttprequest").XMLHttpRequest;

router.get('/login', function (req, res) {
    res.render('admin/login');
});

router.get('/home/:id', function (req, res) {

    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (this.readystate == 4 && this.status == 200) {
            res.render('/admin/home', { admin: JSON.stringify(this.responseText) });
        }
    }

    xhr.open("GET", "http://127.0.0.10/api/Admin/" + req.params.id, true);
    xhr.send();
})

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
                res.redirect('/admin/home/' + body.adminId);
            }
        });
});

module.exports = router;