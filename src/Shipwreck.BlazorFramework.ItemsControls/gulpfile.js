/// <binding BeforeBuild='default' Clean='clean' />
var gulp = require("gulp");
var concat = require('gulp-concat');
var uglify = require('gulp-uglify');
var del = require('del');
var ts = require('gulp-typescript');

gulp.task('clean', function () {
    return del(['wwwroot/*.js']);
});
gulp.task('tsc', function () {
    return gulp.src(['Scripts/*.ts']).pipe(ts({
        outFile: 'Shipwreck.BlazorFramework.ItemsControls.js'
    })).pipe(gulp.dest('wwwroot/'));
});
gulp.task('minify', function () {
    return gulp.src([
        'Scripts/Copyright.js',
        'wwwroot/Shipwreck.BlazorFramework.ItemsControls.js'
    ])
        .pipe(concat('Shipwreck.BlazorFramework.ItemsControls.min.js'))
        .pipe(uglify({
            output: {
                comments: /^!/
            }
        }))
        .pipe(gulp.dest('wwwroot/'));
});
gulp.task('default', gulp.series(['clean', 'tsc', 'minify']));