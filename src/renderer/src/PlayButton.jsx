import React from 'react';
import './PlayButton.css'

export default function PlayButton() {
    return <button className='playButton' type='button' onClick={() => app.window.go()}>Play</button>;
}