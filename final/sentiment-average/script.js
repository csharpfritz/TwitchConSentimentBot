(()=> {
  // set the dimensions and margins of the graph
  let margin = {top: 20, right: 20, bottom: 30, left: 50}
  let width = 500 - margin.left - margin.right
  let height = 100 - margin.top - margin.bottom;

  let x = d3.scaleTime().range([0, width]);
  let y = d3.scaleLinear().range([height, 0]);

  // define the line
  var valueline = d3.line()
      .x(function(d) { return x(d.date); })
      .y(function(d) { return y(d.Sentiment); });

  // parse the date / time
  var parseTime = d3.timeParse("%I:%M");

  // append the svg obgect to the body of the page
  // appends a 'group' element to 'svg'
  // moves the 'group' element to the top left margin
  var svg = d3.select("body").append("svg")
  .attr("width", width + margin.left + margin.right)
  .attr("height", height + margin.top + margin.bottom)
  .append("g")
  .attr("transform",
    "translate(" + margin.left + "," + margin.top + ")");

  // dummy data -- fetch to Fritz's endpoint goes here
  d3.json('./data.json', (err, data) => {
    if (error) throw error;

    // format the data
    data.forEach(function(d) {
        d.time = parseTime(d.time);
        d.Sentiment = +d.Setiment;
    });
  
    // Scale the range of the data
    x.domain(d3.extent(data, function(d) { return d.time; }));
    y.domain([0, d3.max(data, function(d) { return d.Sentiment; })]);

    console.log("domain:", x, y)
  
    // Add the valueline path.
    svg.append("path")
        .data([data])
        .attr("class", "line")
        .attr("d", valueline);
  
    // Add the X Axis
    svg.append("g")
        .attr("transform", "translate(0," + height + ")")
        .call(d3.axisBottom(x));
  
    // Add the Y Axis
    svg.append("g")
        .call(d3.axisLeft(y));
  })
  

})()
