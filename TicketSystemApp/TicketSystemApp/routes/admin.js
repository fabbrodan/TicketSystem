'use strict';
var express = require('express');
var router = express.Router();
var request = require('request');
var XMLHttpRequest = require("xmlhttprequest").XMLHttpRequest;

var periodSale = { theCount: "", Revenue: "" };
var periodSaleDates = { startDate: "", endDate: "" };
var topTen = [{ sumPrice: "", ArtistName: "" }];
var topTenDates = { startDate: "", endDate: "" };
var couponStats = { theCount: "", sumValue: "", ConcertDate: "", ArtistName: "", VenueName: "", ExpirationDate: "" };

router.get('/login', function (req, res) {
    res.render('admin/login');
});

router.get('/home/:id', function (req, res) {

    // Ugly waits but this is to ensure that we load in the data before trying to send it to the template
    // Hacky but will work for now
    // Should implement Promises

    var admin;
    var venues;
    var artists;
    var concerts;

    GetAdmin();
    setTimeout(() => {
    }, 1000);

    GetVenues();
    setTimeout(() => {
    }, 1000);


    GetArtists();
    setTimeout(() => {
    }, 1000);

    GetConcerts();
    setTimeout(() => {
    }, 1000);

    res.render('admin/home', {
        admin: admin,
        venues: venues,
        artists: artists,
        concerts: concerts
    });

    function GetAdmin() {

        var xhr = new XMLHttpRequest();
        xhr.open("GET", "http://127.0.0.10/api/Admin/" + req.params.id, false);

        xhr.onreadystatechange = function () {
            console.log("Admin readystate: " + this.readyState);
            if (this.readyState == 4 && this.status == 200) {
                if (this.responseText != "") {
                    admin = JSON.parse(this.responseText);
                }
            }
        }
        xhr.send();
    }

    function GetVenues() {

        var xhr = new XMLHttpRequest();
        xhr.open("GET", "http://127.0.0.10/api/Venue", false);

        xhr.onreadystatechange = function () {
            console.log("Venues readystate: " + this.readyState);
            if (this.readyState == 4 && this.status == 200) {
                if (this.responseText != "") {
                    venues = JSON.parse(this.responseText);
                }
            }
        }
        xhr.send();
    }

    function GetArtists() {

        var xhr = new XMLHttpRequest();
        xhr.open("GET", "http://127.0.0.10/api/Artist", false);

        xhr.onreadystatechange = function () {
            console.log("Artists readystate: " + this.readyState);
            if (this.readyState == 4 && this.status == 200) {
                if (this.responseText != "") {
                    artists = JSON.parse(this.responseText);
                }
            }
        }
        xhr.send();
    }

    function GetConcerts() {
        var xhr = new XMLHttpRequest();
        xhr.open("POST", "http://127.0.0.10/api/Search/Get?searchParam=", false);

        xhr.onreadystatechange = function () {
            if (this.readyState == 4 && this.status == 200) {
                if (this.responseText != "") {
                    concerts = JSON.parse(this.responseText);
                }
            }
        }
        xhr.send();
    }
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
            else {
                res.redirect('back');
            }
        });
});

router.post('/concertAdd', function (req, res) {
    var artist = parseInt(req.body.artistName);
    var venue = parseInt(req.body.venueName);
    var price = parseFloat(req.body.price);
    var date = req.body.concertDate;
    var time = req.body.concertTime;
    var dateTime = date + "T" + time

    request.post("http://127.0.0.10/api/Concert", {
        json: {
            ArtistId: artist,
            VenueId: venue,
            CalendarDate: dateTime,
            Price: price
        }
    }, (error, response, body) => {
            if (!error && response.statusCode == 200) {
                if (body != null) {
                    res.redirect('/admin/home/' + req.app.locals.globalAdmin.adminId)
                }
                else {
                    res.redirect('/admin/home/' + req.app.locals.globalAdmin.adminId)
                }
            }
            else {
                res.redirect('/admin/home/' + req.app.locals.globalAdmin.adminId)
            }
    });
});

router.post('/artistAdd', function (req, res) {

    request.post("http://127.0.0.10/api/Artist", {
        json: { ArtistName: req.body.artistName }
    }, (error, response, body) => {
            if (!error && response.statusCode == 200) {
                console.log(body);
                res.redirect('/admin/home/' + req.app.locals.globalAdmin.adminId);
            }
            else {
                res.redirect('/admin/home/' + req.app.locals.globalAdmin.adminId);
            }
    });
});

router.post('/adminAdd', function (req, res) {

    request.post("http://127.0.0.10/api/Admin/NewAdmin", {
        json: {
            LoginName: req.body.loginName,
            Email: req.body.email,
            Password: req.body.password
        }
    }, (error, response, body) => {
        if (error || response.statusCode != 200) {
            console.log(error);
        }
    });
    res.redirect('/admin/home/' + req.app.locals.globalAdmin.adminId);

});

router.post('/venueAdd', function (req, res) {

    request.post("http://127.0.0.10/api/Venue", {
        json: {
            VenueName: req.body.venueName,
            City: req.body.venueCity,
            Capacity: parseInt(req.body.venueCapacity)
        }
    }, (error, response, body) => {
            if (!error && response.statusCode == 200) {
                if (body != null) {
                    res.redirect('/admin/home/' + req.app.locals.globalAdmin.adminId);
                }
                else {
                    res.redirect('/admin/home/' + req.app.locals.globalAdmin.adminId);
                }
            } else {
                res.redirect('/admin/home/' + req.app.locals.globalAdmin.adminId);
            }
    });



});

router.post('/reindex', function (req, res) {

    request.post("http://127.0.0.10/api/Index/Index",
        (error, response, body) => {
            if (!error && response.statusCode == 200) {
                res.redirect('/admin/home/' + req.app.locals.globalAdmin.adminId);
            }
            else {
                res.redirect('/admin/home/' + req.app.locals.globalAdmin.adminId);
            }
        });
})

router.post('/cancel', function (req, res) {

    var concertId = parseInt(req.body.concertId);

    request.post("http://127.0.0.10/api/Concert/Cancel/" + concertId, (error, response, body) => {
        if (!error && response.status == 200) {
            res.redirect('/admin/home/' + req.app.locals.globalAdmin.adminId);
        }
        else {
            res.redirect('/admin/home/' + req.app.locals.globalAdmin.adminId);
        }
    });
});

router.get('/reports', function (req, res) {
    res.render('admin/reports', {
        periodSale: periodSale,
        topTen: topTen,
        topTenDates: topTenDates,
        periodSaleDates: periodSaleDates,
        couponStats: couponStats
    });
});

router.post('/periodsales', function (req, res) {
    var startDate = req.body.startDate;
    var endDate = req.body.endDate;

    var xhr = new XMLHttpRequest();
    xhr.open("GET", encodeURI("http://127.0.0.10/api/Reports/PeriodSales?startDate=" + startDate + "&endDate=" + endDate), true);
    xhr.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            periodSale = JSON.parse(this.responseText);
            periodSaleDates = { startDate, endDate };
            res.render('admin/reports', {
                periodSale: periodSale,
                periodSaleDates: periodSaleDates,
                topTen: topTen,
                topTenDates: topTenDates,
                couponStats: couponStats
            });
        }
    }

    xhr.send();
    
});

router.post('/toptenartists', function (req, res) {
    var startDate = req.body.startDate;
    var endDate = req.body.endDate;

    var xhr = new XMLHttpRequest();
    xhr.open("GET", encodeURI("http://127.0.0.10/api/Reports/TopTenArtists?startDate=" + startDate + "&endDate=" + endDate), true);
    xhr.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            topTen = JSON.parse(this.responseText);
            topTenDates = { startDate, endDate };
            res.render('admin/reports', {
                topTen,
                periodSale,
                periodSaleDates,
                topTenDates,
                couponStats: couponStats
            });
        }
    }

    xhr.send();
});

router.post('/couponstats', function (req, res) {

    var xhr = new XMLHttpRequest();
    xhr.open("GET", encodeURI("http://127.0.0.10/api/Reports/CouponReport"), true);
    xhr.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            couponStats = JSON.parse(this.responseText);
            res.render('admin/reports', {
                periodSale: periodSale,
                periodSaleDates: periodSaleDates,
                topTen: topTen,
                topTenDates: topTenDates,
                couponStats: couponStats
            });
        }
    }

    xhr.send();
})

module.exports = router;