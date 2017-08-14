var my_margin = {top: 20, right: 10, bottom: 10, left: 10},
    div_width, width, barWidth,
    barHeight = 20;

var i = 0,
    duration = 300,
    root;

var tree = d3.layout.tree()
    .nodeSize([0, 20])
    .children(function(d){return d.Children})
    .sort(function(a,b) {return a.Name.toLowerCase().localeCompare(b.Name.toLowerCase())});

var diagonal = d3.svg.diagonal()
    .projection(function(d) { return [d.y, d.x]; });

var svg;

Shiny.addCustomMessageHandler("setup_design_configurations", function(message) {
  console.log(typeof svg, svg);
  if(svg === undefined) {
    
    div_width = document.getElementById("design_configurations").offsetWidth;
    //console.log(div_width);
    div_width = 220;
    width = div_width - my_margin.left - my_margin.right;
    barWidth = width * 0.8;
    //console.log(div_width, width, barWidth);
    
    svg = d3.select("#design_configurations").append("svg")
         .attr("id", "design_configurations_svg")
         .attr("width", div_width)
       .append("g")
         .attr("transform", "translate(" + my_margin.left + "," + my_margin.top + ")");
    
    footer_message = message;
    root = jQuery.extend(true, {}, message);
    
    root.x0 = 0;
    root.y0 = 0;
    //select_all(root);
    collapse_default(root);
    
    copy_message(root, footer_message);
    Shiny.onInputChange("filter_design_config_tree", footer_message);
  } else {
    console.log("Error: 'setup_design_configurations' was called more than once from Shiny.");
  }
});

Shiny.addCustomMessageHandler("select_all_design_configurations", function(message) {
  select_all(root);
  update(root, root);
  copy_message(root, footer_message);
  Shiny.onInputChange("filter_design_config_tree", footer_message);
});

// Shiny.addCustomMessageHandler("addnode", function(message) {
//     root = design;
//     var new_object = {};
//     new_object.Name = "Test"
//     new_object.Type = "Component"
//     new_object.Selected = true;
//     root.push(new_object)
//     update(root, root);
// });

function update(root, source) {

  // Compute the flattened node list. TODO use d3.layout.hierarchy.
  var nodes = tree.nodes(root);

  // Workaround to provide sorted list on startup.
  nodes = tree.nodes(root);

  var height = nodes.length * barHeight * 1.3 + my_margin.top + my_margin.bottom;

  d3.select("svg").transition()
      .duration(duration)
      .attr("height", height);

  d3.select(self.frameElement).transition()
      .duration(duration)
      .style("height", height + "px");

  // Compute the "layout".
  nodes.forEach(function(n, i) {
    n.x = i * barHeight * 1.3;
  });

  // Update the nodes…
  var node = svg.selectAll("g.node")
      .data(nodes, function(d) { return d.id || (d.id = ++i); });

  var nodeEnter = node.enter().append("g")
      .attr("class", "node")
      .attr("transform", function(d) { return "translate(" + source.y0 + "," + source.x0 + ")"; })
      .style("opacity", 1e-6);

  // Enter any new nodes at the parent's previous position.
  nodeEnter.append("rect")
      .attr("y", -barHeight / 2)
      .attr("height", barHeight)
      .attr("width", barWidth)
      .attr("rx", 3)
      .attr("ry", 3)
      .style("fill", color)
      .on("click", click);

  nodeEnter.append("text")
      .attr("dy", 3.5)
      .attr("dx", 5.5)
      .text(name);

  // Transition nodes to their new position.
  nodeEnter.transition()
      .duration(duration)
      .attr("transform", function(d) { return "translate(" + d.y + "," + d.x + ")"; })
      .style("opacity", 1);

  node.transition()
      .duration(duration)
      .attr("transform", function(d) { return "translate(" + d.y + "," + d.x + ")"; })
      .style("opacity", 1)
    .select("rect")
      .style("fill", color);

  // Transition exiting nodes to the parent's new position.
  node.exit().transition()
      .duration(duration)
      .attr("transform", function(d) { return "translate(" + source.y + "," + source.x + ")"; })
      .style("opacity", 1e-6)
      .remove();

  // Update the links…
  var link = svg.selectAll("path.link")
      .data(tree.links(nodes), function(d) { return d.target.id; });

  // Enter any new links at the parent's previous position.
  link.enter().insert("path", "g")
      .attr("class", "link")
      .attr("d", function(d) {
        var o = {x: source.x0, y: source.y0};
        return diagonal({source: o, target: o});
      })
    .transition()
      .duration(duration)
      .attr("d", diagonal);

  // Transition links to their new position.
  link.transition()
      .duration(duration)
      .attr("d", diagonal);

  // Transition exiting nodes to the parent's new position.
  link.exit().transition()
      .duration(duration)
      .attr("d", function(d) {
        var o = {x: source.x, y: source.y};
        return diagonal({source: o, target: o});
      })
      .remove();

  // Stash the old positions for transition.
  nodes.forEach(function(d) {
    d.x0 = d.x;
    d.y0 = d.y;
  });
}

// Toggle Children on click.
function click(d) {
  if (d.Children) {
    d._Children = d.Children;
    d.Children = null;
  } else {
    d.Children = d._Children;
    d._Children = null;
  }
  if (d.Selected !== null) {
    if (d.Selected === true) {
      d.Selected = false;
    } else {
      d.Selected = true;
    }
  }
  // Redraw the graph
  update(root, d);
  // Send updated tree to Shiny
  copy_message(root, footer_message);
  Shiny.onInputChange("filter_design_config_tree", footer_message);
}

function copy_message(src, dest) {
  //console.log(src.Name, dest.Name);
  if(dest.hasOwnProperty('Selected')) {
    if (dest.Selected !== src.Selected) {
      console.log("Setting " + dest.Name + " to " + src.Selected);
      dest.Selected = src.Selected;
    }
  } else {
    for (var d in dest.Children) {
      for (var s in src.Children) {
        if (src.Children[s].Name === dest.Children[d].Name) {
          copy_message(src.Children[s], dest.Children[d]);
        }
      }
      for (var s_ in src._Children) {
        if (src._Children[s_].Name === dest.Children[d].Name) {
          copy_message(src._Children[s_], dest.Children[d]);
        }
      }
    }
  }
}

function color(d) {
  if(d.Type == "Compound") return d.Children ? "#F2F1E1" : "#E5E4D5";
  if(d.Type == "Alternative") return d.Children ? "#CBDFBD" : "#C3D6B6";
  if(d.Type == "Optional") return d.Children ? "#E6EFE9" : "#D9E2DC";
  if(d.Type == "Component") return d.Selected === true ? "#D4E09B" : "#F2B79F";
}

function name(d) {
  if(d.Type == "Component") {
    return d.Name;
  } else {
    return d.Name + " (" + d.Type + ")";
  }
}

function collapse(d, max_depth, max_siblings) {
  if(d.Children) {
    d.Children.forEach(function(d) {collapse(d, max_depth-1), max_siblings});
    if (max_depth <= 0 || d.Children.length > max_siblings) {
      d._Children = d.Children;
      d.Children = null;
    }
  }
}

function expand(d) {
  if(d._Children) {
    d.Children = d._Children;
    d._Children = null;
  }
  if(d.Children) {
    d.Children.forEach(expand);
  }
}

function select_all(d) {
  if(d.Children) {
    d.Children.forEach(select_all);
  }
  if(d._Children) {
    d._Children.forEach(select_all);
  }
  if(d.hasOwnProperty('Selected')) {
    d.Selected = true;
  }
}

function collapse_all(root) {
  collapse(root, 0, 0);
  update(root, root);
}

function collapse_default(root) {
  expand(root);
  collapse(root, 2, 3);
  update(root, root);
}

function expand_all(root) {
  expand(root);
  update(root, root);
}

