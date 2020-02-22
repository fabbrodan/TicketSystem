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
    req.app.locals.globalAdmin = null;
    req.app.locals.globalCustomer = null;
    res.redirect('/');
});

router.get('/search', function (req, res) {
    var custId = 0;
    if (req.app.locals.globalCustomer != null) {
        custId = req.app.locals.globalCustomer.customerId;
    }

    res.render('search', { results: {}, customerId: custId, poor: false});
});

router.post('/search', function (req, res) {

    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            res.render('search', { results: JSON.parse(this.responseText), customerId: req.app.locals.globalCustomer != null ? req.app.locals.globalCustomer.customerId : 0, poor: false });
        }
    }

    xhr.open("POST", encodeURI("http://127.0.0.10/api/Search/Get?searchParam=" + req.body.searchTerm), true);
    xhr.send();

});

router.post('/buy', function (req, res) {

    if (req.app.locals.globalCustomer == null) {
        res.render('search', { results: {}, customerId: req.app.locals.globalCustomer != null ? req.app.locals.globalCustomer.customerId : 0, poor: false });
        return;
    }
    
    request.post("http://127.0.0.10/api/Concert/purchase/" + req.body.concertId, {
        json: {
            customerId: req.app.locals.globalCustomer.customerId
        }
    }, (error, response, body) => {
            if (!error && response.statusCode == 200) {
                if (body != "poor") {
                    res.render('search', { results: {}, customerId: req.app.locals.globalCustomer.customerId, poor: false });
                }
                else {
                    res.render('search', { results: {}, customerId: req.app.locals.globalCustomer.customerId, poor: true });
                }
        }
    });
});

module.exports = router;
