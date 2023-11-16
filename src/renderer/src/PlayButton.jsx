import React from 'react';
import './PlayButton.css'

export default function PlayButton({enabled = true, label="Play"}) {
    var classes = 'playButton ';
    if(!enabled)
        classes+="playButton-disabled";
    return <button 
        className={classes} 
        type='button' 
        onClick={enabled? app.launcher.playButtonClick : null}>
        {label}
    </button>;
}