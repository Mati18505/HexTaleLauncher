import React from 'react';
import './ProgressBar.css';

export default function ProgressBar({label, progress}) {
    return <>
    <div className='progress-bar'>
        <div className='progress-bar-fill' style={{width: progress+'%'}}></div>
        <span className='progress-bar-text'>{label}</span>
    </div>;
    </>
    
}