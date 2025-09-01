document.addEventListener('DOMContentLoaded', () => {
    const fetchJsonButton = document.getElementById('fetch-json');
    const fetchXmlButton = document.getElementById('fetch-xml');
    const jsonOutput = document.getElementById('json-output');
    const xmlOutput = document.getElementById('xml-output');

    const fetchData = async (url, outputElement, isJson = false) => {
        outputElement.textContent = 'Loading...';
        try {
            const response = await fetch(url);
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status} ${response.statusText}`);
            }
            const data = isJson ? await response.json() : await response.text();
            outputElement.textContent = isJson ? JSON.stringify(data, null, 2) : data;
        } catch (error) {
            console.error('Fetch error:', error);
            outputElement.textContent = `Error fetching ${url}: ${error.message}`;
        }
    };

    if (fetchJsonButton) {
        fetchJsonButton.addEventListener('click', () => {
            fetchData('/data/data.json', jsonOutput, true);
        });
    }

    if (fetchXmlButton) {
        fetchXmlButton.addEventListener('click', () => {
            fetchData('/data/data.xml', xmlOutput, false);
        });
    }
});

console.log("Script loaded and running!");