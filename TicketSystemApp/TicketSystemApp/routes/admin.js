'use strict';
var express = require('express');
var router = express.Router();
var request = require('request');
var XMLHttpRequest = require("xmlhttprequest").XMLHttpRequest;

router.get('/login', function (req, res) {
    res.render('admin/login');
});

router.get('/home/:id', function (req, res) {

    // RESOLVE THIS SO THAT WE HAVE SEPARATE CALLS INSTEAD OF BEING CUTE ABOUT IT

    var currentCall = 0;
    var urls = ["http://127.0.0.10/api/Admin/" + req.params.id, "http://127.0.0.10/api/Artist", "http://127.0.0.10/api/Venue"];
    var totalCall = urls.length - 1;
    var admin;
    var venues;
    var artists;

    callUrl();

    function callUrl() {
        console.log(urls[currentCall]);
        var xhr = new XMLHttpRequest();
        xhr.open("GET", urls[currentCall], true);

        xhr.send();

        xhr.onreadystatechange = function () {
            if (this.readyState == 4 && this.status == 200) {
                if (currentCall == 0) {
                    admin = JSON.parse(this.responseText);
                    console.log(admin);
                }
                else if (currentCall == 1) {
                    artists = JSON.parse(this.responseText);
                    console.log(artists);
                }
                else if (currentCall == 2) {
                    venues = JSON.parse(this.responseText);
                    console.log(venues);
                }
            }
        }

        while (currentCall < totalCall) {
            currentCall++
            callUrl();
        }
    }

    res.render('admin/home', { admin: admin, venues: venues, artists: artists })
});

router.post('/login', function (req, res) {

    request.post("http://127.0.0.10/api/Admin/Login", {
        json: {
            loginName: req.body.loginId,
            password: req.body.password
        }
    },
        (error, response, body) => {
            if (!error && response.statusCode == 200) {
                if (body != null) {
                    req.app.locals.typeOfAuthenticated = 2;
                    req.app.locals.globalAdmin = body;
                    res.redirect('/admin/home/' + body.adminId);
                }
                else {
                    res.redirect('back');
                }
            }
        });
});

router.post('/concertAdd', function (req, res) {
    var artist = req.body.artistName;
    var venue = req.body.venueName;
    var price = req.body.price;
    var date = req.body.concertDate;
    var time = req.body.concertTime;
    var dateTime = date + "T" + time

    console.log("Artist: " + artist + " Venue: " + venue + " Price: " + price + " DateTime: " + dateTime);
});

router.post('/artistAdd', function (req, res) {

    request.post("http://127.0.0.10/api/Artist", {
        json: { ArtistName: req.body.artistName }
    }, (error, response, body) => {
            if (!error && response.statusCode == 200) {
                console.log(body);
                res.redirect('back');
            }
            else {
                res.redirect('back');
            }
    });
});

router.post('/adminAdd', function (req, res) {

    request.post("http://127.0.0.10/api/Admin", {
        json: {
            LoginName: req.body.loginName,
            Email: req.body.email,
            Password: req.body.password
        }
    }, (error, response, body) => {
        if (error || response.statusCode != 200) {
            console.log("ooops");
        }
    });
    res.redirect('back');

});

router.post('/venueAdd', function (req, res) {

    request.post("http://127.0.0.10/api/Venue", {
        json: {
            VenueName: req.body.venueName,
            City: req.body.venueCity
        }
    }, (error, response, body) => {
        if (!error && response.statusCode == 200) {
            if (body != null) {
                res.redirect('back');
            }
            else {
                console.log("ooops");
                res.redirect('back');
            }
        }
    });


});

module.exports = router;