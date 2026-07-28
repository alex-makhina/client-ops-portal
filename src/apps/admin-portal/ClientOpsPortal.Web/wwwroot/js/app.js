// wwwroot/js/app.js
window.downloadFileFromStream = async (fileName, content) => {
    try {
        console.log('Downloading file:', fileName);
        console.log('Content type:', typeof content);

        // Если content - это StreamReference из Blazor
        if (content && typeof content.arrayBuffer === 'function') {
            const arrayBuffer = await content.arrayBuffer();
            const blob = new Blob([arrayBuffer], { type: 'text/csv;charset=utf-8;' });
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            setTimeout(() => URL.revokeObjectURL(url), 100);
            return true;
        }

        // Если приходит ArrayBuffer
        if (content instanceof ArrayBuffer) {
            const blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            setTimeout(() => URL.revokeObjectURL(url), 100);
            return true;
        }

        // Если приходит Uint8Array
        if (content instanceof Uint8Array) {
            const blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            setTimeout(() => URL.revokeObjectURL(url), 100);
            return true;
        }

        // Если приходит строка
        if (typeof content === 'string') {
            const blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            setTimeout(() => URL.revokeObjectURL(url), 100);
            return true;
        }

        // Если приходит массив байтов (number[])
        if (Array.isArray(content) && content.length > 0 && typeof content[0] === 'number') {
            const byteArray = new Uint8Array(content);
            const blob = new Blob([byteArray], { type: 'text/csv;charset=utf-8;' });
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            setTimeout(() => URL.revokeObjectURL(url), 100);
            return true;
        }

        console.error('Unsupported content type for download:', typeof content, content);
        return false;
    } catch (error) {
        console.error('Error downloading file:', error);
        return false;
    }
};