'use strict';
var express = require('express');
var router = express.Router();
var request = require('request');
var XMLHttpRequest = require("xmlhttprequest").XMLHttpRequest;

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

router.get('/search', function (req, res) {
    res.render('search');
});

router.post('/search', function (req, res) {

    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            res.render('search', { results: JSON.parse(this.responseText) });
        }
    }

    xhr.open("POST", encodeURI("http://127.0.0.10/api/Search/Get?searchParam=" + req.body.searchTerm), true);
    xhr.send();

});

router.post('/buy', function (req, res) {

    if (req.app.locals.customerId == 0) {
        res.render('search', { signedIn: false });
        return;
    }

    request.post("http://127.0.0.10/api/Concert/purchase/" + req.body.concertId, {
        json: {
            customerId: req.app.locals.customerId
        }
    }, (error, response, body) => {
        if (!error && response.statusCode == 200) {
            res.render('search');
        }
    });
});

module.exports = router;
